using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Supabase;

/// <summary>
/// Central authority for the player's XP in a gameplay session.
/// Keeps the XP progress bar in sync and forwards XP deltas to Supabase via XPManager.
/// </summary>
[DisallowMultipleComponent]
public class PlayerXPTracker : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string studentId;
    [SerializeField] private string updatedBy = "GameClient";
    [SerializeField] private bool logDebugMessages;

    [Header("XP State")]
    [SerializeField] private int currentXP;
    [SerializeField] private XPProgressDisplay xpDisplay;
    [SerializeField] private bool setStartingXPOnAwake = false;
    [SerializeField] private int startingXP = 0;

    [Header("Backend")]
    [SerializeField] private Supabase.XPManager xpManager;
    [SerializeField] private Supabase.SupabaseConfig fallbackConfig;

    [Header("Events")]
    public UnityEvent<int> onLevelUp;
    public UnityEvent<int> onXPChanged;

    [Header("VFX (Optional)")]
    [SerializeField] private GameObject xpGainVFX;
    [SerializeField] private GameObject xpDeductionVFX;
    [SerializeField] private GameObject levelUpVFX;
    [SerializeField] private bool spawnVFX = true;
    [SerializeField] private Vector3 xpGainVFXOffset = Vector3.zero; // Spawn at avatar feet
    [SerializeField] private Vector3 xpDeductionVFXOffset = Vector3.up * 1.2f; // Spawn at avatar torso
    [SerializeField] private Vector3 levelUpVFXOffset = Vector3.up * 1.5f; // Spawn above player for level up
    [Tooltip("Manually assign player transform if auto-detection fails. Leave null to auto-detect.")]
    [SerializeField] private Transform playerTransformOverride = null;

    private bool hasInitializedXP = false;
    private Transform cachedPlayerTransform = null; // Cache player transform to avoid repeated searches

    public int CurrentXP => currentXP;
    public string StudentId => studentId;

    void Awake()
    {
        if (xpDisplay == null)
        {
            xpDisplay = FindObjectOfType<XPProgressDisplay>();
        }

        if (xpManager == null)
        {
            xpManager = FindObjectOfType<Supabase.XPManager>();
        }

        if (xpManager != null && xpManager.config == null && fallbackConfig != null)
        {
            xpManager.config = fallbackConfig;
        }
        
        // Try to get student ID from PlayerPrefs if not already set
        if (string.IsNullOrEmpty(studentId))
        {
            string prefStudentId = PlayerPrefs.GetString("CurrentUserId", "");
            if (!string.IsNullOrEmpty(prefStudentId))
            {
                studentId = prefStudentId;
                if (logDebugMessages)
                    Debug.Log($"PlayerXPTracker: Loaded student ID from PlayerPrefs: {studentId}");
            }
        }

        if (setStartingXPOnAwake && !hasInitializedXP)
        {
            SetXPImmediate(startingXP);
        }
        else
        {
            ApplyXPToDisplay();
        }
    }

    /// <summary>
    /// Call once after loading profile data so the tracker knows the player's starting state.
    /// </summary>
    public void InitializeFromProfile(Supabase.Student student)
    {
        if (student == null)
            return;

        studentId = student.id;
        SetXPImmediate(student.xp);
    }

    /// <summary>
    /// Immediately sets XP without reporting to backend (useful for initial load).
    /// </summary>
    public void SetXPImmediate(int totalXP)
    {
        currentXP = Mathf.Max(0, totalXP);
        hasInitializedXP = true;
        ApplyXPToDisplay();
    }

    /// <summary>
    /// Allows other systems (e.g., HUD spawner) to inject the XP display instance at runtime.
    /// </summary>
    public void SetXPDisplay(XPProgressDisplay display)
    {
        xpDisplay = display;
        ApplyXPToDisplay();
    }

    /// <summary>
    /// Grants or deducts XP, updates HUD, and reports to Supabase.
    /// Supports negative amounts for XP deduction.
    /// </summary>
    public void GrantXP(int amount, string reason = "Puzzle solved")
    {
        if (amount == 0)
            return;

        // Allow negative values for XP deduction (but ensure XP doesn't go below 0)
        int previousLevel = LevelCalculator.CalculateLevel(currentXP);
        int previousXP = currentXP;
        int actualDelta = amount;
        
        // Calculate what the XP would be before clamping
        int newXPValue = currentXP + amount;
        currentXP = Mathf.Max(0, newXPValue); // Prevent XP from going below 0
        
        // If XP was clamped to 0, adjust the delta to reflect actual change for display
        // But still send the original amount to database to track the penalty attempt
        if (currentXP == 0 && newXPValue < 0)
        {
            // For local tracking, use the actual amount deducted (clamped)
            actualDelta = -previousXP;
            if (logDebugMessages)
                Debug.Log($"PlayerXPTracker: XP deduction ({amount}) would result in negative XP. Current XP: {previousXP}. Deducting {previousXP} XP (clamped to 0). Penalty still recorded in database.");
        }

        // Only update display if XP actually changed locally
        if (currentXP != previousXP)
        {
            ApplyXPToDisplay();
            onXPChanged?.Invoke(currentXP);

            int newLevel = LevelCalculator.CalculateLevel(currentXP);
            if (newLevel > previousLevel)
            {
                onLevelUp?.Invoke(newLevel);
                // Spawn level up VFX at torso/above player
                SpawnVFX(levelUpVFX, levelUpVFXOffset, "Level Up!");
            }
            else
            {
                // Spawn appropriate VFX based on whether XP was gained or deducted
                if (amount > 0)
                {
                    // XP gain VFX spawns at avatar feet
                    SpawnVFX(xpGainVFX, xpGainVFXOffset, "XP Gained");
                }
                else if (amount < 0)
                {
                    // XP deduction VFX spawns at avatar torso
                    SpawnVFX(xpDeductionVFX, xpDeductionVFXOffset, "XP Deducted");
                }
            }
        }

        // ALWAYS send the ORIGINAL amount to database to track penalties
        // This ensures penalties are recorded even if player has insufficient XP
        if (!string.IsNullOrEmpty(studentId) && xpManager != null && xpManager.config != null)
        {
            // Use original amount for database (not the clamped actualDelta)
            StartCoroutine(ReportXPDelta(amount, reason));
        }
        else if (logDebugMessages)
        {
            Debug.LogWarning("PlayerXPTracker: Missing student ID or XPManager config. XP will not sync to Supabase.");
        }
    }

    private void ApplyXPToDisplay()
    {
        if (xpDisplay != null)
        {
            xpDisplay.SetXP(currentXP);
        }
    }

    private IEnumerator ReportXPDelta(int amount, string reason)
    {
        bool completed = false;
        string backendError = null;
        
        // Log what we're sending
        if (logDebugMessages || amount < 0)
        {
            string xpType = amount >= 0 ? "XP gain" : "XP penalty";
            Debug.Log($"PlayerXPTracker: Reporting {xpType} - Amount: {amount}, Reason: {reason}");
        }

        yield return xpManager.AddXpEvent(studentId, amount, reason, updatedBy, (error) =>
        {
            backendError = error;
            completed = true;
        });

        if (!completed)
            yield break;

        if (!string.IsNullOrEmpty(backendError) && logDebugMessages)
        {
            Debug.LogError($"PlayerXPTracker: Failed to report XP. {backendError}");
        }
        else if (logDebugMessages)
        {
            Debug.Log($"PlayerXPTracker: Reported {amount} XP ({reason}) for {studentId}.");
        }
    }

    /// <summary>
    /// Spawns a VFX effect that follows the player's movement with a specific local offset.
    /// </summary>
    private void SpawnVFX(GameObject vfxPrefab, Vector3 localOffset, string debugName = "")
    {
        if (!spawnVFX || vfxPrefab == null)
            return;

        if (VFXManager.Instance == null)
        {
            if (logDebugMessages)
                Debug.LogWarning("PlayerXPTracker: VFXManager.Instance is null. Cannot spawn VFX.");
            return;
        }

        // Get player transform to parent the VFX
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null)
        {
            if (logDebugMessages)
                Debug.LogWarning("PlayerXPTracker: Could not find player transform. VFX will spawn at world position.");
            // Fallback: spawn at world position
            Vector3 worldPosition = GetFallbackPosition() + localOffset;
            VFXManager.Instance.SpawnVFX(vfxPrefab, worldPosition, Quaternion.identity, null, 3f);
            return;
        }

        if (logDebugMessages && !string.IsNullOrEmpty(debugName))
        {
            Debug.Log($"PlayerXPTracker: Spawning {debugName} VFX at player with local offset {localOffset}");
        }

        // Spawn VFX at player's position, then parent it to player so it follows movement
        // We'll set the local position after parenting to ensure correct offset
        Vector3 spawnPosition = playerTransform.position;
        GameObject vfxInstance = VFXManager.Instance.SpawnVFX(vfxPrefab, spawnPosition, Quaternion.identity, playerTransform, 3f);
        
        // Set local position to the offset so it stays relative to player
        // This ensures the VFX appears at the correct position (feet/torso) relative to the player
        if (vfxInstance != null)
        {
            vfxInstance.transform.localPosition = localOffset;
        }
    }

    /// <summary>
    /// Gets the player's transform for parenting VFX.
    /// Uses multiple fallback methods and caches the result.
    /// </summary>
    private Transform GetPlayerTransform()
    {
        // First check manual override
        if (playerTransformOverride != null)
        {
            cachedPlayerTransform = playerTransformOverride;
            return cachedPlayerTransform;
        }

        // Return cached transform if available
        if (cachedPlayerTransform != null)
            return cachedPlayerTransform;

        // Method 1: Find by "Player" tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            cachedPlayerTransform = player.transform;
            return cachedPlayerTransform;
        }

        // Method 2: Find PlayerController component
        var playerController = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerController>();
        if (playerController != null)
        {
            cachedPlayerTransform = playerController.transform;
            return cachedPlayerTransform;
        }

        // Method 3: Find CharacterController component (common player component)
        CharacterController characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
        {
            cachedPlayerTransform = characterController.transform;
            return cachedPlayerTransform;
        }

        // Method 4: Try to find player via camera parent (if camera is child of player)
        if (Camera.main != null)
        {
            Transform cameraParent = Camera.main.transform.parent;
            if (cameraParent != null)
            {
                // Check if parent has CharacterController or PlayerController
                if (cameraParent.GetComponent<CharacterController>() != null ||
                    cameraParent.GetComponent<UserAvatar.FinalCharacterController.PlayerController>() != null)
                {
                    cachedPlayerTransform = cameraParent;
                    return cachedPlayerTransform;
                }
            }
        }

        // No player found
        return null;
    }

    /// <summary>
    /// Clears the cached player transform, forcing a new search on next VFX spawn.
    /// Useful if player is destroyed/recreated during gameplay.
    /// </summary>
    public void RefreshPlayerTransform()
    {
        cachedPlayerTransform = null;
    }

    /// <summary>
    /// Gets a fallback position if player is not found (camera or this object).
    /// </summary>
    private Vector3 GetFallbackPosition()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform.position;
        }
        return transform.position;
    }
}

