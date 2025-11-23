using UnityEngine;

/// <summary>
/// Tracks the initial spawn position of the player and provides functionality
/// to reset the player to this position (for unstuck feature).
/// </summary>
public class PlayerSpawnTracker : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Position")]
    [SerializeField] private bool useCurrentPositionAsSpawn = true;
    [SerializeField] private Vector3 customSpawnPosition;
    [SerializeField] private bool resetRotationOnSpawn = true;
    [SerializeField] private Vector3 spawnRotation = Vector3.zero;

    private Vector3 spawnPosition;
    private Quaternion spawnRotationQuaternion;

    // Singleton for easy access
    public static PlayerSpawnTracker Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find player if not assigned
        if (playerTransform == null)
        {
            // Try to find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Try to find PlayerController
                var playerController = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerController>();
                if (playerController != null)
                {
                    playerTransform = playerController.transform;
                }
            }
        }

        // Determine spawn position
        if (useCurrentPositionAsSpawn && playerTransform != null)
        {
            spawnPosition = playerTransform.position;
            spawnRotationQuaternion = playerTransform.rotation;
        }
        else
        {
            spawnPosition = customSpawnPosition;
            spawnRotationQuaternion = Quaternion.Euler(spawnRotation);
        }

        // If still no player found, log warning
        if (playerTransform == null)
        {
            Debug.LogWarning("PlayerSpawnTracker: Player transform not found. Make sure player is tagged 'Player' or has PlayerController component.");
        }
    }

    /// <summary>
    /// Resets the player to the initial spawn position and rotation.
    /// </summary>
    public void ResetPlayerToSpawn()
    {
        if (playerTransform == null)
        {
            Debug.LogError("PlayerSpawnTracker: Cannot reset player - player transform is null.");
            return;
        }

        // Get CharacterController if present to handle proper movement
        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        
        if (characterController != null)
        {
            // Disable CharacterController temporarily to allow position change
            characterController.enabled = false;
        }

        // Reset position
        playerTransform.position = spawnPosition;

        // Reset rotation if enabled
        if (resetRotationOnSpawn)
        {
            playerTransform.rotation = spawnRotationQuaternion;
        }

        // Re-enable CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Reset player velocity if PlayerController exists
        var playerController = playerTransform.GetComponent<UserAvatar.FinalCharacterController.PlayerController>();
        if (playerController != null)
        {
            // Velocity is private, so we'll need to rely on CharacterController.Move reset
            // The position reset should handle this
        }

        Debug.Log($"PlayerSpawnTracker: Player reset to spawn position: {spawnPosition}");
    }

    /// <summary>
    /// Updates the spawn position to the player's current position.
    /// Useful if you want to update spawn after entering a new area.
    /// </summary>
    public void UpdateSpawnPosition()
    {
        if (playerTransform != null)
        {
            spawnPosition = playerTransform.position;
            spawnRotationQuaternion = playerTransform.rotation;
            Debug.Log($"PlayerSpawnTracker: Spawn position updated to: {spawnPosition}");
        }
    }

    /// <summary>
    /// Gets the current spawn position.
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

    /// <summary>
    /// Sets a custom spawn position.
    /// </summary>
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }

    /// <summary>
    /// Sets a custom spawn position and rotation.
    /// </summary>
    public void SetSpawnTransform(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotationQuaternion = rotation;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws gizmo in editor to show spawn position.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 displayPos = useCurrentPositionAsSpawn && Application.isPlaying 
            ? spawnPosition : customSpawnPosition;
            
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(displayPos, 0.5f);
        Gizmos.DrawLine(displayPos, displayPos + Vector3.up * 2f);
    }
#endif
}

