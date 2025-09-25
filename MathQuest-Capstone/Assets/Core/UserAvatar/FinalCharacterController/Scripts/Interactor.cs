using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Transform interactionSource; // usually the Camera
    public float interactRange = 3f;
    public LayerMask hitMask = ~0;

    public bool TryInteract()
    {
        if (!interactionSource) return false;

        var ray = new Ray(interactionSource.position, interactionSource.forward);

        if (Physics.Raycast(ray, out var hit, interactRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.TryGetComponent<IInteractable>(out var it))
            {
                it.Interact();
                return true;
            }
        }
        return false;
    }
}
