using UnityEngine;

/// <summary>
/// Example: A simple door that can be opened/closed.
/// Can be combined with PuzzleLockedInteractable to require a puzzle.
/// </summary>
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioSource audioSource;

    private Quaternion targetRotation;
    private bool isAnimating = false;

    void Start()
    {
        targetRotation = transform.localRotation;
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (isAnimating) return;

        isOpen = !isOpen;
        
        if (isOpen)
            Open();
        else
            Close();
    }

    private void Open()
    {
        targetRotation = Quaternion.Euler(0, openAngle, 0);
        isAnimating = true;
        
        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);
        
        Debug.Log($"{gameObject.name}: Door opening");
    }

    private void Close()
    {
        targetRotation = Quaternion.Euler(0, closeAngle, 0);
        isAnimating = true;
        
        if (audioSource && closeSound)
            audioSource.PlayOneShot(closeSound);
        
        Debug.Log($"{gameObject.name}: Door closing");
    }

    void Update()
    {
        if (isAnimating)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, 
                targetRotation, 
                Time.deltaTime * openSpeed
            );

            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
            }
        }
    }

    // Public methods for external control
    public void ForceOpen() { isOpen = true; Open(); }
    public void ForceClose() { isOpen = false; Close(); }
    public bool IsOpen => isOpen;
}

