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
    }

    public void ResetReward()
    {
        hasAwarded = false;
    }
}

