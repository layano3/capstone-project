using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example: A lever that can be pulled/pushed.
/// Can be combined with PuzzleLockedInteractable to require a puzzle.
/// </summary>
public class LeverInteractable : MonoBehaviour, IInteractable
{
    [Header("Lever Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool canToggle = true;
    [SerializeField] private float activatedAngle = -45f;
    [SerializeField] private float deactivatedAngle = 45f;
    [SerializeField] private float pullSpeed = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip pullSound;
    [SerializeField] private AudioSource audioSource;

    private Quaternion targetRotation;
    private bool isAnimating = false;

    void Start()
    {
        UpdateTargetRotation();
        transform.localRotation = targetRotation;
        
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (isAnimating) return;

        if (!canToggle && isActivated)
        {
            Debug.Log($"{gameObject.name}: Lever is already activated and cannot be toggled.");
            return;
        }

        isActivated = !isActivated;
        UpdateTargetRotation();
        isAnimating = true;

        if (audioSource && pullSound)
            audioSource.PlayOneShot(pullSound);

        // Invoke events
        if (isActivated)
        {
            Debug.Log($"{gameObject.name}: Lever activated!");
            onActivated?.Invoke();
        }
        else
        {
            Debug.Log($"{gameObject.name}: Lever deactivated!");
            onDeactivated?.Invoke();
        }
    }

    private void UpdateTargetRotation()
    {
        float angle = isActivated ? activatedAngle : deactivatedAngle;
        targetRotation = Quaternion.Euler(angle, 0, 0);
    }

    void Update()
    {
        if (isAnimating)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * pullSpeed
            );

            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
            }
        }
    }

    // Public methods for external control
    public void Activate() { isActivated = true; UpdateTargetRotation(); isAnimating = true; onActivated?.Invoke(); }
    public void Deactivate() { isActivated = false; UpdateTargetRotation(); isAnimating = true; onDeactivated?.Invoke(); }
    public bool IsActivated => isActivated;
}

