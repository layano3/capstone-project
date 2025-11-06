using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Source & Range")]
    public Transform interactionSource; // usually the Camera
    public float interactRange = 8f;

    [Header("Layers")]
    public LayerMask hitMask = ~0;
    public LayerMask ignoreMask = 0; // exclude player colliders, etc.

    [Header("Debug")]
    public bool showDebugRay = true;
    public Color debugRayColor = Color.red;

    [Header("UI Integration")]
    public InteractionUI interactionUI; // expects Show(anchor) / Hide()
    [Tooltip("Time in seconds to prevent showing prompt again after interaction")]
    public float interactionCooldown = 0.5f;

    // === NEW: public read-only hover state ===
    public IInteractable CurrentTarget { get; private set; }
    public Transform CurrentAnchor { get; private set; }  // where to place the E
    private float lastInteractionTime = -999f;

    void Awake()
    {
        if (!interactionSource) SetupCameraReference();
        if (!interactionUI) interactionUI = FindObjectOfType<InteractionUI>();
    }

    void SetupCameraReference()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            interactionSource = mainCamera.transform;
            Debug.Log($"Interactor: Auto-assigned camera '{mainCamera.name}' as interaction source");
            return;
        }

        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length > 0)
        {
            interactionSource = cameras[0].transform;
            Debug.Log($"Interactor: Auto-assigned camera '{cameras[0].name}' as interaction source");
        }
        else
        {
            Debug.LogWarning("Interactor: No camera found! Please assign interactionSource manually.");
        }
    }

    void Update()
    {
        // Existing input (unchanged)
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();

        // === Passive hover detection for visuals ===
        UpdateHoverVisual();
    }

    void UpdateHoverVisual()
    {
        if (!interactionSource || !interactionUI)
        {
            ClearHover();
            return;
        }

        // Don't show prompt if we just interacted (cooldown)
        if (Time.time < lastInteractionTime + interactionCooldown)
        {
            ClearHover();
            return;
        }

        // Try to find a target WITHOUT calling Interact()
        if (TryFindTarget(out var target, out var anchor, out var hitInfo))
        {
            if (CurrentTarget != target)
            {
                CurrentTarget = target;
                CurrentAnchor = anchor;
                if (interactionUI)
                    interactionUI.Show(CurrentAnchor != null ? CurrentAnchor : hitInfo.collider.transform);
            }
            else
            {
                // keep following the same anchor if it moves
                CurrentAnchor = anchor;
                if (interactionUI && interactionUI.IsVisible)
                    interactionUI.Follow(CurrentAnchor != null ? CurrentAnchor : hitInfo.collider.transform);
            }
        }
        else
        {
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget = null;
            CurrentAnchor = null;
            if (interactionUI) interactionUI.Hide();
        }
    }

    // === Public: call this when player presses E (or your input) ===
    public bool TryInteract()
    {
        if (!interactionSource) return false;

        // Prefer the hovered target if it exists and is still valid
        if (CurrentTarget != null)
        {
            CurrentTarget.Interact();
            lastInteractionTime = Time.time;
            // Hide the prompt after interaction
            if (interactionUI) interactionUI.Hide();
            ClearHover();
            return true;
        }

        // Otherwise, actively probe and interact
        if (ProbeOnce(activate: true, out _, out _))
        {
            lastInteractionTime = Time.time;
            // Hide the prompt after interaction
            if (interactionUI) interactionUI.Hide();
            ClearHover();
            return true;
        }

        return false;
    }

    // === NEW: unified passive finder ===
    bool TryFindTarget(out IInteractable target, out Transform anchor, out RaycastHit hit)
    {
        target = null;
        anchor = null;
        hit = default;

        // Use the same probe logic but do NOT activate
        if (ProbeOnce(activate: false, out var t, out var h))
        {
            target = t;
            hit = h;
            anchor = GetAnchorFor(t, h.collider.transform);
            return true;
        }
        return false;
    }

    Transform GetAnchorFor(IInteractable target, Transform fallback)
    {
        // If your interactables have a specific anchor child, expose it via interface or marker
        // Try a common pattern first:
        var anchor = (target as Component)?.GetComponentInChildren<InteractablePromptAnchor>(true);
        return anchor ? anchor.transform : fallback;
    }

    // === Core probe that reuses your Raycast/Sphere/Cone in order ===
    bool ProbeOnce(bool activate, out IInteractable found, out RaycastHit foundHit)
    {
        found = null;
        foundHit = default;

        if (TryRaycast(out found, out foundHit, activate)) return true;
        if (TrySphereCast(out found, out foundHit, activate)) return true;
        if (TryConeRaycast(out found, out foundHit, activate)) return true;

        return false;
    }

    bool TryRaycast(out IInteractable it, out RaycastHit hit, bool activate)
    {
        it = null;
        hit = default;
        var ray = new Ray(interactionSource.position, interactionSource.forward);

        if (showDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactRange, debugRayColor, 0.1f);

        LayerMask combinedMask = hitMask & ~ignoreMask;

        if (Physics.Raycast(ray, out hit, interactRange, combinedMask, QueryTriggerInteraction.Ignore))
        {
            if (IsSelf(hit)) return false;

            if (showDebugRay)
                Debug.Log($"Raycast hit: {hit.collider.name} at distance {hit.distance}");

            it = hit.collider.GetComponentInParent<IInteractable>();
            if (it != null)
            {
                if (activate) it.Interact();
                return true;
            }
        }
        else if (showDebugRay)
        {
            Debug.Log("Raycast missed - no objects in range");
        }
        return false;
    }

    bool TrySphereCast(out IInteractable it, out RaycastHit hit, bool activate)
    {
        it = null;
        hit = default;

        var ray = new Ray(interactionSource.position, interactionSource.forward);

        if (showDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow, 0.1f);

        LayerMask combinedMask = hitMask & ~ignoreMask;

        if (Physics.SphereCast(ray, 0.3f, out hit, interactRange, combinedMask, QueryTriggerInteraction.Ignore))
        {
            if (IsSelf(hit)) return false;

            if (showDebugRay)
                Debug.Log($"SphereCast hit: {hit.collider.name} at distance {hit.distance}");

            it = hit.collider.GetComponentInParent<IInteractable>();
            if (it != null)
            {
                if (activate) it.Interact();
                return true;
            }
        }
        return false;
    }

    bool TryConeRaycast(out IInteractable it, out RaycastHit hit, bool activate)
    {
        it = null;
        hit = default;

        Vector3 baseDirection = interactionSource.forward;
        float coneAngle = 15f; // degrees
        int rayCount = 5;

        LayerMask combinedMask = hitMask & ~ignoreMask;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i - rayCount / 2f) * coneAngle / rayCount;
            Vector3 direction = Quaternion.AngleAxis(angle, interactionSource.up) * baseDirection;
            var ray = new Ray(interactionSource.position, direction);

            if (showDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green, 0.1f);

            if (Physics.Raycast(ray, out var h, interactRange, combinedMask, QueryTriggerInteraction.Ignore))
            {
                if (IsSelf(h)) continue;

                it = h.collider.GetComponentInParent<IInteractable>();
                if (it != null)
                {
                    if (showDebugRay)
                        Debug.Log($"Cone raycast hit: {h.collider.name} at distance {h.distance}");

                    if (activate) it.Interact();
                    hit = h;
                    return true;
                }
            }
        }
        return false;
    }

    bool IsSelf(RaycastHit hit)
    {
        return hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform;
    }
}
