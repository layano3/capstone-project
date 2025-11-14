using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Utility to drop an XP progress prefab into any gameplay scene and position it in the bottom-left corner.
/// Add this to a manager object in your scene (e.g., BlackSmithEnv) and assign the prefab containing XPProgressDisplay.
/// </summary>
[DisallowMultipleComponent]
public class XPProgressHUDInstaller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private XPProgressDisplay xpProgressPrefab;
    [SerializeField] private XPProgressDisplay existingInstance;

    [Header("Layout")]
    [SerializeField] private Vector2 bottomLeftOffset = new Vector2(48f, 48f);
    [SerializeField] private Vector2 sizeDelta = new Vector2(420f, 96f);
    [SerializeField] [Min(0.1f)] private float uniformScale = 1f;
    [SerializeField] private bool overrideSize = false;
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool dontDestroyOnLoad;
    [SerializeField] private bool forceNewCanvas = true;

    public XPProgressDisplay Instance => existingInstance;

    void Start()
    {
        if (setupOnStart)
        {
            SetupHUD();
        }
    }

    /// <summary>
    /// Creates or reuses the XP progress bar and positions it bottom-left on the target canvas.
    /// </summary>
    public XPProgressDisplay SetupHUD()
    {
        if (existingInstance == null)
        {
            Canvas canvas = EnsureCanvas();
            if (canvas == null)
                return null;

            if (xpProgressPrefab == null)
            {
                Debug.LogError($"XPProgressHUDInstaller on '{name}' needs an XPProgressDisplay prefab assigned.");
                return null;
            }

            existingInstance = Instantiate(xpProgressPrefab, canvas.transform);
            existingInstance.name = xpProgressPrefab.name;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(existingInstance.gameObject);
            }
        }

        PositionBottomLeft(existingInstance.GetComponent<RectTransform>());
        NotifyTracker(existingInstance);
        return existingInstance;
    }

    private Canvas EnsureCanvas()
    {
        if (targetCanvas == null)
        {
            if (!forceNewCanvas)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }

            if (targetCanvas == null)
            {
                targetCanvas = CreateGameplayCanvas();
            }
        }

        return targetCanvas;
    }

    private Canvas CreateGameplayCanvas()
    {
        GameObject canvasGO = new GameObject("GameplayHUDCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        return canvas;
    }

    private void PositionBottomLeft(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = bottomLeftOffset;
        rect.localScale = Vector3.one * uniformScale;

        if (overrideSize)
        {
            rect.sizeDelta = sizeDelta;
        }
    }

    private void NotifyTracker(XPProgressDisplay display)
    {
        if (display == null)
            return;

        var tracker = FindObjectOfType<PlayerXPTracker>();
        if (tracker != null)
        {
            tracker.SetXPDisplay(display);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Setup XP Progress HUD")]
    private void ContextSetup()
    {
        if (Application.isPlaying)
            SetupHUD();
        else
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                SetupHUD();
                UnityEditor.EditorUtility.SetDirty(gameObject);
            };
    }
#endif
}

