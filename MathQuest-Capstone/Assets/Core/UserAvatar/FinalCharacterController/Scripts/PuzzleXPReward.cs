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

    public void ResetReward()
    {
        hasAwarded = false;
    }
}

