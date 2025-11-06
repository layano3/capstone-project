using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas interactionCanvas;
    public GameObject interactionPrompt;
    public TextMeshProUGUI interactionText;
    
    [Header("Settings")]
    public string interactionKey = "E";
    public string interactionMessage = "Press {0} to interact";
    public float fadeSpeed = 5f;
    public float scaleSpeed = 5f;
    
    [Header("Animation")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 2f;
    public float bobbingAmount = 0.1f;
    
    private Camera playerCamera;
    private Interactor interactor;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private bool isVisible = false;
    private float bobbingOffset = 0f;
    
    void Start()
    {
        InitializeUI();
    }
    
    void InitializeUI()
    {
        Debug.Log("SimpleInteractionUI: Initializing...");
        
        // Setup canvas group
        if (interactionCanvas != null)
        {
            canvasGroup = interactionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = interactionCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f; // Start hidden
        }
        
        // Store original scale
        if (interactionPrompt != null)
        {
            originalScale = interactionPrompt.transform.localScale;
            interactionPrompt.SetActive(false); // Start hidden
        }
        
        // Setup text
        if (interactionText != null)
        {
            interactionText.text = string.Format(interactionMessage, interactionKey);
        }
        
        // Find camera and interactor
        playerCamera = Camera.main ?? FindObjectOfType<Camera>();
        interactor = FindObjectOfType<Interactor>();
        
        Debug.Log($"SimpleInteractionUI: Initialized - Camera: {playerCamera?.name}, Interactor: {interactor?.name}");
    }
    
    void Update()
    {
        if (playerCamera == null || interactor == null || interactionCanvas == null || interactionPrompt == null)
        {
            return;
        }
        
        // Check if we should show the interaction prompt
        bool shouldShow = CheckForInteractable();
        
        if (shouldShow != isVisible)
        {
            SetVisibility(shouldShow);
        }
        
        if (isVisible)
        {
            UpdateUI();
        }
    }
    
    bool CheckForInteractable()
    {
        if (!interactor.interactionSource) return false;
        
        // Use the same logic as the Interactor to check for interactables
        var ray = new Ray(interactor.interactionSource.position, interactor.interactionSource.forward);
        LayerMask combinedMask = interactor.hitMask & ~interactor.ignoreMask;
        
        if (Physics.Raycast(ray, out var hit, interactor.interactRange, combinedMask, QueryTriggerInteraction.Ignore))
        {
            // Check if it's not the player itself
            if (hit.collider.transform.IsChildOf(interactor.transform) || hit.collider.transform == interactor.transform)
            {
                return false;
            }
            
            // Check if it has an IInteractable component
            if (hit.collider.gameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void UpdateUI()
    {
        if (playerCamera == null) return;
        
        // Position the UI in world space
        Vector3 worldPosition = GetInteractablePosition();
        Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);
        
        // Check if the object is in front of the camera
        bool isInFront = screenPosition.z > 0;
        
        if (isInFront)
        {
            // Convert to UI space
            RectTransform canvasRect = interactionCanvas.GetComponent<RectTransform>();
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, playerCamera, out uiPosition);
            
            interactionPrompt.transform.localPosition = uiPosition;
            
            // Update bobbing animation
            if (enableBobbing)
            {
                bobbingOffset += Time.deltaTime * bobbingSpeed;
                float bobbingY = Mathf.Sin(bobbingOffset) * bobbingAmount;
                Vector3 currentPos = interactionPrompt.transform.localPosition;
                interactionPrompt.transform.localPosition = new Vector3(currentPos.x, currentPos.y + bobbingY, currentPos.z);
            }
        }
    }
    
    Vector3 GetInteractablePosition()
    {
        if (!interactor.interactionSource) return Vector3.zero;
        
        var ray = new Ray(interactor.interactionSource.position, interactor.interactionSource.forward);
        LayerMask combinedMask = interactor.hitMask & ~interactor.ignoreMask;
        
        if (Physics.Raycast(ray, out var hit, interactor.interactRange, combinedMask, QueryTriggerInteraction.Ignore))
        {
            // Return a position slightly above the hit point
            return hit.point + Vector3.up * 1f;
        }
        
        return Vector3.zero;
    }
    
    void SetVisibility(bool visible)
    {
        if (interactionPrompt == null || canvasGroup == null) return;
        
        isVisible = visible;
        
        if (visible)
        {
            interactionPrompt.SetActive(true);
            StartCoroutine(FadeIn());
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null || interactionPrompt == null) yield break;
        
        float targetAlpha = 1f;
        Vector3 targetScale = originalScale;
        
        while (canvasGroup.alpha < targetAlpha)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            interactionPrompt.transform.localScale = Vector3.Lerp(interactionPrompt.transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        interactionPrompt.transform.localScale = targetScale;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup == null || interactionPrompt == null) yield break;
        
        float targetAlpha = 0f;
        Vector3 targetScale = originalScale * 0.8f;
        
        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            interactionPrompt.transform.localScale = Vector3.Lerp(interactionPrompt.transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        interactionPrompt.SetActive(false);
        interactionPrompt.transform.localScale = originalScale;
    }
    
    // Public method to update interaction key
    public void SetInteractionKey(string key)
    {
        interactionKey = key;
        if (interactionText != null)
        {
            interactionText.text = string.Format(interactionMessage, interactionKey);
        }
    }
}
