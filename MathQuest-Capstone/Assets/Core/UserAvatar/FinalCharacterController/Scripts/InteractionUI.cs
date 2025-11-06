using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private GameObject interactionPrompt;   // root RectTransform or world-space container
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Image interactionIcon;

    [Header("Visual Prefab")]
    [Tooltip("Use a prefab instead of auto-generated UI. If set, this will be instantiated and used as the prompt.")]
    [SerializeField] private GameObject promptPrefab;
    [SerializeField] private bool usePrefabInstead = false;

    [Header("Settings")]
    [SerializeField] private string interactionKey = "E";
    [SerializeField] private string interactionMessage = "Press {0} to interact";
    [SerializeField] private float fadeSpeed = 10f;          // faster, feels snappier
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Follow / Anchor")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.0f, 0f); // added above target
    [SerializeField] private bool clampToScreen = true;                      // avoid disappearing at edges
    [SerializeField] private Vector2 screenPadding = new Vector2(16, 16);

    [Header("Animation")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingAmount = 10f; // in UI pixels for screen-space; in meters for world-space

    [Header("Distance Scaling")]
    [SerializeField] private bool enableDistanceScaling = true;
    [SerializeField] private float minScale = 0.5f; // scale when far away
    [SerializeField] private float maxScale = 1.2f; // scale when close
    [SerializeField] private float minDistance = 2f; // distance at which maxScale is applied
    [SerializeField] private float maxDistance = 8f; // distance at which minScale is applied

    private Camera playerCamera;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private bool _isVisible = false;
    private float bobbingT = 0f;

    // NEW: supplied by Interactor (or your hover system)
    private Transform _anchor;               // world-space anchor to follow
    private Coroutine _fadeRoutine;

    public bool IsVisible => _isVisible;

    void Awake()
    {
        // Ensure canvas reference exists (prefer local Canvas)
        if (!interactionCanvas)
        {
            interactionCanvas = GetComponent<Canvas>();
            if (!interactionCanvas)
            {
                // Try parent canvas
                interactionCanvas = GetComponentInParent<Canvas>();
            }
            if (!interactionCanvas)
            {
                // Create a simple overlay canvas on this GameObject
                interactionCanvas = gameObject.AddComponent<Canvas>();
                interactionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // CanvasGroup for fading
        if (interactionCanvas)
        {
            canvasGroup = interactionCanvas.GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = interactionCanvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // start hidden
        }

        // Ensure a prompt root exists
        if (!interactionPrompt)
        {
            // If using prefab, instantiate it
            if (usePrefabInstead && promptPrefab != null)
            {
                // Instantiate prefab as child of canvas
                interactionPrompt = Instantiate(promptPrefab, interactionCanvas.transform);
                interactionPrompt.name = "InteractionPrompt";
                
                // Ensure it has a RectTransform and set it up properly
                RectTransform prefabRect = interactionPrompt.GetComponent<RectTransform>();
                if (prefabRect == null)
                {
                    prefabRect = interactionPrompt.AddComponent<RectTransform>();
                }
                
                // Center it on the canvas
                prefabRect.anchorMin = new Vector2(0.5f, 0.5f);
                prefabRect.anchorMax = new Vector2(0.5f, 0.5f);
                prefabRect.anchoredPosition = Vector2.zero;
                
                // Try to find text component in prefab
                if (!interactionText)
                    interactionText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                // Try to find any suitable child as prompt
                var rt = GetComponentInChildren<RectTransform>();
                if (rt != null && rt.gameObject != gameObject)
                {
                    interactionPrompt = rt.gameObject;
                }
                else
                {
                    // Create a basic prompt container
                    var promptGO = new GameObject("InteractionPrompt");
                    promptGO.transform.SetParent(transform, false);
                    var prt = promptGO.AddComponent<RectTransform>();
                    prt.anchorMin = new Vector2(0.5f, 0.5f);
                    prt.anchorMax = new Vector2(0.5f, 0.5f);
                    prt.anchoredPosition = Vector2.zero;
                    prt.sizeDelta = new Vector2(250f, 80f);
                    var bg = promptGO.AddComponent<Image>();
                    bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    interactionPrompt = promptGO;
                }
            }
        }

        // Ensure TMP text exists and shows default
        if (!interactionText && interactionPrompt)
        {
            interactionText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (!interactionText)
            {
                var textGO = new GameObject("InteractionText");
                textGO.transform.SetParent(interactionPrompt.transform, false);
                interactionText = textGO.AddComponent<TextMeshProUGUI>();
                interactionText.fontSize = 24;
                interactionText.color = Color.white;
                interactionText.alignment = TextAlignmentOptions.Center;
                interactionText.fontStyle = FontStyles.Bold;
                var tr = textGO.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.offsetMin = Vector2.zero;
                tr.offsetMax = Vector2.zero;
            }
        }

        if (interactionText)
            interactionText.text = string.Format(interactionMessage, interactionKey);

        if (interactionPrompt)
            originalScale = interactionPrompt.transform.localScale;

        // Camera
        playerCamera = Camera.main ? Camera.main : FindObjectOfType<Camera>();

        // Hide initially
        interactionPrompt?.SetActive(false);
        _isVisible = false;
    }

    void Start()
    {
        if (interactionText)
            interactionText.text = string.Format(interactionMessage, interactionKey);
    }

    void LateUpdate()
    {
        if (!_isVisible || _anchor == null || playerCamera == null || interactionCanvas == null || interactionPrompt == null)
            return;

        // Calculate distance-based scale
        float distanceScale = 1f;
        if (enableDistanceScaling && playerCamera != null && _anchor != null)
        {
            float distance = Vector3.Distance(playerCamera.transform.position, _anchor.position);
            float normalizedDistance = Mathf.InverseLerp(minDistance, maxDistance, distance);
            distanceScale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        }

        // Apply distance scaling to the prompt
        if (enableDistanceScaling)
        {
            Vector3 targetScale = originalScale * distanceScale;
            interactionPrompt.transform.localScale = Vector3.Lerp(interactionPrompt.transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
        }

        // Follow anchor
        switch (interactionCanvas.renderMode)
        {
            case RenderMode.WorldSpace:
                // Place the prompt in world-space, billboard to camera
                interactionPrompt.transform.position = _anchor.position + worldOffset;
                // Billboard
                interactionPrompt.transform.LookAt(interactionPrompt.transform.position + playerCamera.transform.forward,
                                                   playerCamera.transform.up);
                // Bobbing in meters
                if (enableBobbing)
                {
                    bobbingT += Time.unscaledDeltaTime * bobbingSpeed;
                    var pos = interactionPrompt.transform.position;
                    pos.y = (_anchor.position + worldOffset).y + Mathf.Sin(bobbingT) * bobbingAmount;
                    interactionPrompt.transform.position = pos;
                }
                break;

            case RenderMode.ScreenSpaceOverlay:
            case RenderMode.ScreenSpaceCamera:
                // Convert world to screen → canvas local
                Vector3 screenPos = playerCamera.WorldToScreenPoint(_anchor.position + worldOffset);
                bool inFront = screenPos.z > 0f;

                // Optional screen clamp
                if (clampToScreen)
                {
                    screenPos.x = Mathf.Clamp(screenPos.x, screenPadding.x, Screen.width - screenPadding.x);
                    screenPos.y = Mathf.Clamp(screenPos.y, screenPadding.y, Screen.height - screenPadding.y);
                }

                RectTransform canvasRect = interactionCanvas.GetComponent<RectTransform>();
                RectTransform promptRect = interactionPrompt.transform as RectTransform;

                if (inFront && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screenPos,
                        interactionCanvas.renderMode == RenderMode.ScreenSpaceCamera ? interactionCanvas.worldCamera : null,
                        out Vector2 local))
                {
                    promptRect.anchoredPosition = local;

                    if (enableBobbing)
                    {
                        bobbingT += Time.unscaledDeltaTime * bobbingSpeed;
                        Vector2 bob = new Vector2(0f, Mathf.Sin(bobbingT) * bobbingAmount);
                        promptRect.anchoredPosition = local + bob;
                    }
                }
                else
                {
                    // If behind camera, hide softly
                    Hide();
                }
                break;
        }
    }

    // ===== PUBLIC API (call these from Interactor/hover system) =====
    public void Show(Transform anchor)
    {
        _anchor = anchor;
        if (_anchor == null)
        {
            Hide();
            return;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            _isVisible = true;
            StartFade(targetAlpha: 1f, targetScale: originalScale);
        }
        else
        {
            Debug.LogWarning("InteractionUI: interactionPrompt is null! Cannot show prompt.");
        }
    }

    public void Hide()
    {
        _isVisible = false;
        StartFade(targetAlpha: 0f, targetScale: originalScale * 0.9f, deactivateOnEnd: true);
        _anchor = null;
    }

    public void Follow(Transform anchor)
    {
        _anchor = anchor; // safe to update while visible
    }

    // ===== Helpers =====
    void StartFade(float targetAlpha, Vector3 targetScale, bool deactivateOnEnd = false)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, targetScale, deactivateOnEnd));
    }

    IEnumerator FadeRoutine(float targetAlpha, Vector3 targetScale, bool deactivateOnEnd)
    {
        if (!canvasGroup || !interactionPrompt) yield break;

        float t = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = interactionPrompt.transform.localScale;

        while (true)
        {
            t += Time.unscaledDeltaTime * fadeSpeed;
            float u = Mathf.Clamp01(t);

            // Smooth interpolation; separate scale speed for extra snap
            float s = 1f - Mathf.Pow(1f - u, 3f); // ease out
            float ss = Mathf.Clamp01(Time.unscaledDeltaTime * scaleSpeed + s * (1f - Time.unscaledDeltaTime * scaleSpeed));

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, s);
            interactionPrompt.transform.localScale = Vector3.Lerp(startScale, targetScale, ss);

            if (u >= 1f) break;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        interactionPrompt.transform.localScale = targetScale;

        if (deactivateOnEnd && Mathf.Approximately(targetAlpha, 0f))
            interactionPrompt.SetActive(false);

        _fadeRoutine = null;
    }

    // Optional: external setters you already had
    public void SetInteractionKey(string key)
    {
        interactionKey = key;
        if (interactionText)
            interactionText.text = string.Format(interactionMessage, interactionKey);
    }

    public void SetInteractionMessage(string message)
    {
        interactionMessage = message;
        if (interactionText)
            interactionText.text = string.Format(interactionMessage, interactionKey);
    }
}
