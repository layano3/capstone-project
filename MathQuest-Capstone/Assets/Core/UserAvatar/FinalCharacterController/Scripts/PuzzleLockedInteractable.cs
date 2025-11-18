using UnityEngine;
using System;

/// <summary>
/// Generic wrapper that adds puzzle-locking functionality to ANY IInteractable.
/// This component intercepts interactions and shows a puzzle before allowing the actual interaction.
/// Can be added to chests, doors, levers, NPCs, or any other interactable object.
/// </summary>
[DisallowMultipleComponent]
public class PuzzleLockedInteractable : MonoBehaviour, IInteractable
{
    [Header("Puzzle Lock Settings")]
    [Tooltip("Enable to require puzzle completion before interaction")]
    [SerializeField] private bool isLocked = true;
    
    [Tooltip("Reference to the puzzle GameObject (will automatically find the IPuzzle component on it)")]
    [SerializeField] private GameObject puzzleGameObject;
    
    [Tooltip("If true, puzzle only needs to be solved once. If false, puzzle resets after each interaction.")]
    [SerializeField] private bool solveOnce = true;
    
    [Tooltip("If true, interaction will be completely disabled after puzzle is solved. If false, user can still interact after solving.")]
    [SerializeField] private bool disableInteractionAfterSolve = true;
    
    [Tooltip("Allow manual unlock/lock via code or other triggers")]
    [SerializeField] private bool canBeManuallyUnlocked = true;

    [Header("Messages")]
    [SerializeField] private string lockedMessage = "This is locked! Solve the puzzle to unlock it.";
    [SerializeField] private string unlockedMessage = "Unlocked!";
    [SerializeField] private bool showDebugMessages = true;

    [Header("Events")]
    [Tooltip("Optional: Invoke Unity Events when puzzle is completed")]
    [SerializeField] private UnityEngine.Events.UnityEvent onPuzzleCompleted;
    [Tooltip("Optional: Invoke Unity Events when puzzle is cancelled")]
    [SerializeField] private UnityEngine.Events.UnityEvent onPuzzleCancelled;
    [Tooltip("Optional: Invoke Unity Events when manually unlocked")]
    [SerializeField] private UnityEngine.Events.UnityEvent onManuallyUnlocked;

    [Header("Rewards")]
    [SerializeField] private bool awardXPOnSolve = false;
    [SerializeField] private int xpRewardAmount = 25;
    [SerializeField] private string xpRewardReason = "Puzzle solved";
    [SerializeField] private bool awardXPOnlyOnce = true;
    [SerializeField] private PlayerXPTracker xpTracker;

    private IPuzzle puzzle;
    private IInteractable wrappedInteractable;
    private bool hasBeenUnlocked = false;
    private Collider[] interactableColliders; // Store colliders to disable them later
    private bool xpAwarded = false;
    private int myQuestionIndex = 0; // Track which question this interactable is on

    public bool IsLocked => isLocked && !hasBeenUnlocked;
    public bool HasBeenUnlocked => hasBeenUnlocked;

    void Awake()
    {
        // Find the puzzle reference
        SetupPuzzleReference();
        
        // Find the actual interactable we're wrapping
        SetupWrappedInteractable();
        
        // Cache colliders that the Interactor might raycast against
        CacheInteractableColliders();

        if (xpTracker == null && awardXPOnSolve)
        {
            xpTracker = FindObjectOfType<PlayerXPTracker>();
        }
    }

    private void CacheInteractableColliders()
    {
        // Find all colliders on this GameObject and its children
        // These are what the Interactor raycasts against
        interactableColliders = GetComponentsInChildren<Collider>();
        
        if (showDebugMessages && interactableColliders.Length > 0)
        {
            Debug.Log($"PuzzleLockedInteractable on '{gameObject.name}': Found {interactableColliders.Length} collider(s) for interaction detection.");
        }
    }

    private void SetupPuzzleReference()
    {
        if (puzzleGameObject != null)
        {
            // Find IPuzzle component on the assigned GameObject
            puzzle = puzzleGameObject.GetComponent<IPuzzle>() as IPuzzle;
            
            if (puzzle == null)
            {
                // Try MonoBehaviour components that might implement IPuzzle
                var components = puzzleGameObject.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp is IPuzzle)
                    {
                        puzzle = comp as IPuzzle;
                        break;
                    }
                }
            }
            
            if (puzzle == null)
            {
                Debug.LogError($"PuzzleLockedInteractable on '{gameObject.name}': The assigned puzzle GameObject '{puzzleGameObject.name}' does not have a component that implements IPuzzle interface!");
            }
            else if (showDebugMessages)
            {
                Debug.Log($"PuzzleLockedInteractable on '{gameObject.name}': Found puzzle component on '{puzzleGameObject.name}'");
            }
        }
        else if (isLocked)
        {
            // Try to find puzzle in scene
            var puzzleInScene = FindObjectOfType<SimpleMathPuzzle>();
            if (puzzleInScene != null)
            {
                puzzle = puzzleInScene as IPuzzle;
                puzzleGameObject = puzzleInScene.gameObject;
                if (showDebugMessages)
                    Debug.Log($"PuzzleLockedInteractable on '{gameObject.name}': Auto-found puzzle in scene.");
            }
            else
            {
                Debug.LogWarning($"PuzzleLockedInteractable on '{gameObject.name}': No puzzle assigned and none found in scene!");
            }
        }
    }

    private void SetupWrappedInteractable()
    {
        // Find other IInteractable components on this GameObject
        var interactables = GetComponents<IInteractable>();
        
        foreach (var interactable in interactables)
        {
            // Skip ourselves
            if (interactable == this as IInteractable)
                continue;
            
            // Found the wrapped interactable
            wrappedInteractable = interactable;
            if (showDebugMessages)
                Debug.Log($"PuzzleLockedInteractable on '{gameObject.name}': Wrapping {interactable.GetType().Name}");
            break;
        }

        if (wrappedInteractable == null && showDebugMessages)
        {
            Debug.LogWarning($"PuzzleLockedInteractable on '{gameObject.name}': No other IInteractable found to wrap. This component will only show puzzles.");
        }
    }

    /// <summary>
    /// Called when player interacts with this object.
    /// If locked, shows puzzle. If unlocked and interaction not disabled, calls the wrapped interactable.
    /// </summary>
    public void Interact()
    {
        // If interaction is disabled after solve and puzzle is already solved, do nothing
        if (disableInteractionAfterSolve && hasBeenUnlocked)
        {
            if (showDebugMessages)
                Debug.Log($"{gameObject.name}: Interaction disabled - puzzle already solved.");
            return;
        }

        // If not locked or already unlocked, interact normally
        if (!isLocked || hasBeenUnlocked)
        {
            InteractWithWrappedObject();
            return;
        }

        // Show puzzle
        if (showDebugMessages)
            Debug.Log($"{gameObject.name} is locked! Showing puzzle...");
        
        ShowLockedMessage();
        ShowPuzzle();
    }

    private void ShowPuzzle()
    {
        if (puzzle == null)
        {
            Debug.LogWarning($"No puzzle available for {gameObject.name}!");
            return;
        }

        // If puzzle is already active (from another interactable), don't show it again
        if (puzzle.IsActive)
        {
            if (showDebugMessages)
                Debug.Log($"{gameObject.name}: Puzzle is already active from another interactable. Waiting...");
            return;
        }

        // Pass our question index to the puzzle FIRST, before resetting
        // This ensures the puzzle knows which question to show for this interactable
        SetPuzzleQuestionIndex(myQuestionIndex);

        // Reset puzzle completion state when showing for this interactable
        // This ensures each interactable gets a fresh puzzle, even if they share the same puzzle instance
        // Note: We set the index before resetting, so ResetPuzzle won't override it
        if (puzzle.IsCompleted)
        {
            // Temporarily store the index, reset, then restore it
            int savedIndex = myQuestionIndex;
            puzzle.ResetPuzzle();
            SetPuzzleQuestionIndex(savedIndex);
        }

        puzzle.ShowPuzzle(
            onComplete: OnPuzzleCompleted,
            onCancel: OnPuzzleCancelled
        );
    }

    private void SetPuzzleQuestionIndex(int index)
    {
        // Use reflection to set the question index on the puzzle
        // This works for both SimpleMathPuzzle and MultipleChoicePuzzle
        var puzzleType = puzzle.GetType();
        var field = puzzleType.GetField("sequentialQuestionIndex", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(puzzle, index);
        }
        else
        {
            // Try alternative field name for custom questions
            field = puzzleType.GetField("sequentialCustomQuestionIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(puzzle, index);
            }
        }
    }

    private void OnPuzzleCompleted()
    {
        if (showDebugMessages)
            Debug.Log($"Puzzle completed! {gameObject.name} is now unlocked.");
        
        hasBeenUnlocked = true;
        
        // Advance to next question for this interactable
        if (!solveOnce)
        {
            myQuestionIndex++;
        }
        
        ShowUnlockedMessage();
        
        // Invoke Unity Event
        onPuzzleCompleted?.Invoke();

        AwardXPReward();
        
        // Automatically interact with the wrapped object (opens chest once)
        InteractWithWrappedObject();
        
        // If interaction should be disabled after solve, disable everything
        if (disableInteractionAfterSolve)
        {
            if (showDebugMessages)
                Debug.Log($"{gameObject.name}: Interaction disabled after puzzle completion.");
            
            // Disable this component so it no longer responds to interactions
            enabled = false;
            
            // Also disable the wrapped interactable so the prompt doesn't show
            if (wrappedInteractable != null && wrappedInteractable is MonoBehaviour)
            {
                (wrappedInteractable as MonoBehaviour).enabled = false;
            }
            
            // Disable all colliders so the Interactor can't detect this object at all
            // This prevents the "Press E" prompt from appearing
            DisableInteractableColliders();
        }
    }

    private void OnPuzzleCancelled()
    {
        if (showDebugMessages)
            Debug.Log($"Puzzle cancelled. {gameObject.name} remains locked.");
        
        // Invoke Unity Event
        onPuzzleCancelled?.Invoke();
    }

    private void InteractWithWrappedObject()
    {
        if (wrappedInteractable != null)
        {
            wrappedInteractable.Interact();
        }
        else if (showDebugMessages)
        {
            Debug.Log($"{gameObject.name}: No wrapped interactable to call.");
        }
    }

    private void ShowLockedMessage()
    {
        if (!string.IsNullOrEmpty(lockedMessage) && showDebugMessages)
        {
            Debug.Log($"{gameObject.name}: {lockedMessage}");
        }
        // TODO: Integrate with your UI message system
    }

    private void ShowUnlockedMessage()
    {
        if (!string.IsNullOrEmpty(unlockedMessage) && showDebugMessages)
        {
            Debug.Log($"{gameObject.name}: {unlockedMessage}");
        }
        // TODO: Integrate with your UI message system
    }

    private void AwardXPReward()
    {
        if (!awardXPOnSolve)
            return;

        if (xpTracker == null)
        {
            if (showDebugMessages)
                Debug.LogWarning($"{gameObject.name}: Cannot award XP - no PlayerXPTracker assigned.");
            return;
        }

        if (awardXPOnlyOnce && xpAwarded)
            return;

        xpTracker.GrantXP(Mathf.Max(0, xpRewardAmount), string.IsNullOrEmpty(xpRewardReason) ? "Puzzle solved" : xpRewardReason);
        xpAwarded = true;
    }

    private void DisableInteractableColliders()
    {
        if (interactableColliders != null && interactableColliders.Length > 0)
        {
            foreach (var collider in interactableColliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            
            if (showDebugMessages)
            {
                Debug.Log($"{gameObject.name}: Disabled {interactableColliders.Length} collider(s) - interaction prompt will no longer appear.");
            }
        }
    }

    private void EnableInteractableColliders()
    {
        if (interactableColliders != null && interactableColliders.Length > 0)
        {
            foreach (var collider in interactableColliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
            
            if (showDebugMessages)
            {
                Debug.Log($"{gameObject.name}: Re-enabled {interactableColliders.Length} collider(s).");
            }
        }
    }

    // ===== PUBLIC API FOR EXTERNAL CONTROL =====

    /// <summary>
    /// Manually unlock this interactable without requiring puzzle completion.
    /// </summary>
    public void UnlockManually()
    {
        if (!canBeManuallyUnlocked)
        {
            Debug.LogWarning($"Cannot manually unlock {gameObject.name} - manual unlocking is disabled.");
            return;
        }

        hasBeenUnlocked = true;
        if (showDebugMessages)
            Debug.Log($"{gameObject.name} manually unlocked.");
        
        onManuallyUnlocked?.Invoke();
    }

    /// <summary>
    /// Lock this interactable again (requires puzzle to be solved again).
    /// </summary>
    public void Lock()
    {
        hasBeenUnlocked = false;
        
        // Re-enable components and colliders if they were disabled
        enabled = true;
        
        if (wrappedInteractable != null && wrappedInteractable is MonoBehaviour)
        {
            (wrappedInteractable as MonoBehaviour).enabled = true;
        }
        
        EnableInteractableColliders();
        
        if (showDebugMessages)
            Debug.Log($"{gameObject.name} locked and re-enabled.");
    }

    /// <summary>
    /// Toggle the lock state.
    /// </summary>
    public void ToggleLock()
    {
        if (hasBeenUnlocked)
            Lock();
        else
            UnlockManually();
    }

    /// <summary>
    /// Enable or disable the puzzle requirement entirely.
    /// </summary>
    public void SetLockEnabled(bool enabled)
    {
        isLocked = enabled;
        if (showDebugMessages)
            Debug.Log($"{gameObject.name} lock requirement set to: {enabled}");
    }

    /// <summary>
    /// Reset the puzzle and lock state.
    /// </summary>
    public void ResetLock()
    {
        hasBeenUnlocked = false;
        myQuestionIndex = 0; // Reset question index
        
        // Re-enable components and colliders
        enabled = true;
        
        if (wrappedInteractable != null && wrappedInteractable is MonoBehaviour)
        {
            (wrappedInteractable as MonoBehaviour).enabled = true;
        }
        
        EnableInteractableColliders();
        
        if (puzzle != null && !solveOnce)
        {
            puzzle.ResetPuzzle();
        }

        if (!awardXPOnlyOnce)
        {
            xpAwarded = false;
        }
        if (showDebugMessages)
            Debug.Log($"{gameObject.name} lock reset and re-enabled.");
    }

    // ===== EDITOR HELPERS =====

    void OnValidate()
    {
        if (puzzleGameObject != null)
        {
            // Check if the GameObject has an IPuzzle component
            var components = puzzleGameObject.GetComponents<MonoBehaviour>();
            bool hasIPuzzle = false;
            foreach (var comp in components)
            {
                if (comp is IPuzzle)
                {
                    hasIPuzzle = true;
                    break;
                }
            }
            
            if (!hasIPuzzle)
            {
                Debug.LogWarning($"The assigned puzzle GameObject '{puzzleGameObject.name}' on '{gameObject.name}' does not have a component that implements IPuzzle interface!");
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Unlock")]
    private void TestUnlock()
    {
        UnlockManually();
    }

    [ContextMenu("Test Lock")]
    private void TestLock()
    {
        Lock();
    }

    [ContextMenu("Test Reset")]
    private void TestReset()
    {
        ResetLock();
    }
#endif
}

