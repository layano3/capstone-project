using UnityEngine;

/// <summary>
/// Helper that can be triggered from PuzzleLockedInteractable's UnityEvent to award XP.
/// Attach to the same object as the puzzle or interactable, configure XP amount,
/// and hook GrantReward() into the OnPuzzleCompleted event in the inspector.
/// </summary>
public class PuzzleXPReward : MonoBehaviour
{
    [SerializeField] private PlayerXPTracker xpTracker;
    [SerializeField] private int xpAmount = 25;
    [SerializeField] private string reason = "Puzzle solved";
    [SerializeField] private bool awardOnlyOnce = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip xpGainSound;
    [SerializeField] private AudioClip puzzleSolvedSound;
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private bool playSounds = true;

    [Header("VFX (Optional)")]
    [SerializeField] private GameObject xpGainVFX;
    [SerializeField] private GameObject puzzleSolvedVFX;
    [SerializeField] private bool spawnVFX = true;
    [SerializeField] private Vector3 vfxOffset = Vector3.up * 1.5f; // Spawn above puzzle/interactable by default

    private bool hasAwarded;

    private void Awake()
    {
        if (xpTracker == null)
        {
            xpTracker = FindObjectOfType<PlayerXPTracker>();
        }
    }

    public void GrantReward()
    {
        if (awardOnlyOnce && hasAwarded)
            return;

        if (xpTracker == null)
        {
            Debug.LogWarning($"PuzzleXPReward on '{name}' requires a PlayerXPTracker reference.");
            return;
        }

        xpTracker.GrantXP(xpAmount, reason);
        hasAwarded = true;
        
        // Play sound effect
        PlayXPSound();
        
        // Spawn visual effect
        SpawnXPVFX();
    }
    
    private void PlayXPSound()
    {
        if (!playSounds) return;
        
        // Prefer puzzle solved sound if available, otherwise use generic XP sound
        AudioClip soundToPlay = puzzleSolvedSound != null ? puzzleSolvedSound : xpGainSound;
        
        if (soundToPlay == null) return;
        
        Vector3 position = transform.position;
        
        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXOneShot(soundToPlay, position, 0.7f);
        }
        else
        {
            // Fallback: try to find AudioSource on this object
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(soundToPlay, 0.7f);
            }
        }
    }

    private void SpawnXPVFX()
    {
        if (!spawnVFX) return;
        
        // Prefer puzzle solved VFX if available, otherwise use generic XP gain VFX
        GameObject vfxToSpawn = puzzleSolvedVFX != null ? puzzleSolvedVFX : xpGainVFX;
        
        if (vfxToSpawn == null) return;
        
        Vector3 spawnPosition = transform.position + vfxOffset;
        
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnVFX(vfxToSpawn, spawnPosition, Quaternion.identity, null, 3f);
        }
        else
        {
            // Fallback: instantiate directly if VFXManager is not available
            GameObject instance = Instantiate(vfxToSpawn, spawnPosition, Quaternion.identity);
            Destroy(instance, 3f);
        }
    }

    public void ResetReward()
    {
        hasAwarded = false;
    }
}

