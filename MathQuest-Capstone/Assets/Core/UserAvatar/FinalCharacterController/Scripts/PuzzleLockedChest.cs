using UnityEngine;

/// <summary>
/// A chest that requires solving a puzzle before it can be opened.
/// Extends the basic ChestAnimationInteractable with puzzle functionality.
/// </summary>
[RequireComponent(typeof(Animation)), RequireComponent(typeof(Collider))]
public class PuzzleLockedChest : MonoBehaviour, IInteractable
{
    [Header("Animation (Legacy)")]
    [SerializeField] private Animation anim;
    [SerializeField] private AnimationClip openClip;
    [SerializeField] private AnimationClip closeClip;

    [Header("Behavior")]
    [SerializeField] private bool toggle = true;
    [SerializeField] private bool onlyOnce = false;
    [SerializeField] private float cooldown = 0.25f;
    [SerializeField] private float crossFade = 0.1f;

    [Header("Puzzle System")]
    [SerializeField] private bool requiresPuzzle = true;
    [Tooltip("Reference to the puzzle component (can be on this object or another object)")]
    [SerializeField] private MonoBehaviour puzzleComponent;
    [Tooltip("Show this message when chest is locked")]
    [SerializeField] private string lockedMessage = "This chest is locked! Solve the puzzle to open it.";

    private IPuzzle puzzle;
    private bool isUnlocked = false;
    private bool opened = false;
    private float lastUse;

    void Awake()
    {
        // Setup animation
        if (!anim) anim = GetComponent<Animation>();
        anim.playAutomatically = false;
        anim.enabled = true;

        AddClipIfNeeded(openClip);
        AddClipIfNeeded(closeClip);

        SetWrap(openClip);
        SetWrap(closeClip);

        // Setup puzzle reference
        if (puzzleComponent != null)
        {
            puzzle = puzzleComponent as IPuzzle;
            if (puzzle == null)
            {
                Debug.LogError($"PuzzleLockedChest: The assigned puzzle component on '{puzzleComponent.name}' does not implement IPuzzle interface!");
            }
        }
        else if (requiresPuzzle)
        {
            // Try to find puzzle in scene
            puzzle = FindObjectOfType<SimpleMathPuzzle>() as IPuzzle;
            if (puzzle == null)
            {
                Debug.LogWarning($"PuzzleLockedChest: No puzzle assigned and none found in scene! Chest will not require puzzle.");
                requiresPuzzle = false;
            }
        }
    }

    void AddClipIfNeeded(AnimationClip clip)
    {
        if (clip && anim.GetClip(clip.name) == null)
            anim.AddClip(clip, clip.name);
    }

    void SetWrap(AnimationClip clip)
    {
        if (clip) anim[clip.name].wrapMode = WrapMode.ClampForever;
    }

    public void Interact()
    {
        if (Time.time < lastUse + cooldown) return;
        lastUse = Time.time;

        // Check if puzzle is required and not yet unlocked
        if (requiresPuzzle && !isUnlocked)
        {
            Debug.Log("Chest is locked! Showing puzzle...");
            ShowLockedMessage();
            ShowPuzzle();
            return;
        }

        // Proceed with normal chest interaction
        if (onlyOnce && opened) return;
        if (!openClip) return;

        if (toggle && opened && closeClip)
        {
            Play(closeClip.name);
            opened = false;
        }
        else
        {
            Play(openClip.name);
            opened = true;
        }
    }

    private void ShowPuzzle()
    {
        if (puzzle == null)
        {
            Debug.LogWarning("No puzzle available!");
            return;
        }

        // Check if puzzle was already completed
        if (puzzle.IsCompleted)
        {
            OnPuzzleCompleted();
            return;
        }

        puzzle.ShowPuzzle(
            onComplete: OnPuzzleCompleted,
            onCancel: OnPuzzleCancelled
        );
    }

    private void OnPuzzleCompleted()
    {
        Debug.Log("Puzzle completed! Chest is now unlocked.");
        isUnlocked = true;
        
        // Automatically open the chest after puzzle completion
        if (openClip)
        {
            Play(openClip.name);
            opened = true;
        }

        // Optional: Show unlock feedback
        ShowUnlockedMessage();
    }

    private void OnPuzzleCancelled()
    {
        Debug.Log("Puzzle cancelled. Chest remains locked.");
    }

    void Play(string name)
    {
        if (anim.GetClip(name) == null) return;
        SetWrap(openClip);
        SetWrap(closeClip);
        if (crossFade > 0f && anim.isPlaying) anim.CrossFade(name, crossFade);
        else anim.Play(name);
    }

    private void ShowLockedMessage()
    {
        // You can integrate this with your UI system
        Debug.Log(lockedMessage);
        // TODO: Show UI message to player
    }

    private void ShowUnlockedMessage()
    {
        // You can integrate this with your UI system
        Debug.Log("Chest unlocked!");
        // TODO: Show UI message to player
    }

    // Public methods for external control
    public void UnlockChest()
    {
        isUnlocked = true;
        Debug.Log("Chest manually unlocked.");
    }

    public void LockChest()
    {
        isUnlocked = false;
        opened = false;
        Debug.Log("Chest manually locked.");
    }

    public bool IsLocked => requiresPuzzle && !isUnlocked;
    public bool IsOpened => opened;

    // Editor helper
    void OnValidate()
    {
        if (puzzleComponent != null && !(puzzleComponent is IPuzzle))
        {
            Debug.LogWarning($"The assigned puzzle component does not implement IPuzzle interface!");
        }
    }
}

