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

    private bool hasInitializedXP = false;

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
    /// Grants XP, updates HUD, and reports to Supabase.
    /// </summary>
    public void GrantXP(int amount, string reason = "Puzzle solved")
    {
        if (amount == 0)
            return;

        if (amount < 0)
        {
            if (logDebugMessages)
                Debug.LogWarning($"PlayerXPTracker: Negative XP ({amount}) ignored.");
            return;
        }

        int previousLevel = LevelCalculator.CalculateLevel(currentXP);
        currentXP += amount;
        ApplyXPToDisplay();

        onXPChanged?.Invoke(currentXP);

        int newLevel = LevelCalculator.CalculateLevel(currentXP);
        if (newLevel > previousLevel)
        {
            onLevelUp?.Invoke(newLevel);
        }

        if (!string.IsNullOrEmpty(studentId) && xpManager != null && xpManager.config != null)
        {
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
}

