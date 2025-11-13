using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example: An NPC that can be talked to.
/// Can be combined with PuzzleLockedInteractable to require a puzzle before conversation.
/// </summary>
public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string[] dialogueLines = new string[] 
    { 
        "Hello, traveler!",
        "How can I help you today?",
        "Safe travels!"
    };
    [SerializeField] private int currentLineIndex = 0;
    [SerializeField] private bool loopDialogue = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onDialogueStart;
    [SerializeField] private UnityEvent onDialogueEnd;
    [SerializeField] private UnityEvent onInteract;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip talkSound;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        onInteract?.Invoke();

        if (dialogueLines.Length == 0)
        {
            Debug.Log($"{npcName}: (No dialogue configured)");
            return;
        }

        // Start dialogue
        if (currentLineIndex == 0)
        {
            onDialogueStart?.Invoke();
        }

        // Show current line
        string currentLine = dialogueLines[currentLineIndex];
        Debug.Log($"{npcName}: {currentLine}");
        ShowDialogue(currentLine);

        // Play sound
        if (audioSource && talkSound)
            audioSource.PlayOneShot(talkSound);

        // Advance to next line
        currentLineIndex++;

        // Check if dialogue is finished
        if (currentLineIndex >= dialogueLines.Length)
        {
            if (loopDialogue)
            {
                currentLineIndex = 0;
            }
            else
            {
                onDialogueEnd?.Invoke();
            }
        }
    }

    private void ShowDialogue(string text)
    {
        // TODO: Integrate with your dialogue UI system
        // For now, just log to console
        Debug.Log($"[Dialogue] {npcName}: {text}");
    }

    // Public methods for external control
    public void ResetDialogue()
    {
        currentLineIndex = 0;
    }

    public void SetDialogue(string[] newDialogue)
    {
        dialogueLines = newDialogue;
        currentLineIndex = 0;
    }

    public string GetCurrentLine()
    {
        if (dialogueLines.Length == 0) return "";
        return dialogueLines[Mathf.Clamp(currentLineIndex, 0, dialogueLines.Length - 1)];
    }
}

