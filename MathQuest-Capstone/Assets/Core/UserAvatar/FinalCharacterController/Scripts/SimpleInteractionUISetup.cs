using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Reflection;

public class SimpleInteractionUISetup : MonoBehaviour
{
    [Header("Setup")]
    public bool createOnStart = true;
    
    [Header("UI References")]
    public Canvas targetCanvas;
    public GameObject steampunkFramePrefab;
    
    [Header("Settings")]
    public string interactionKey = "E";
    public string interactionMessage = "Press {0} to interact";
    
    void Start()
    {
        if (createOnStart)
        {
            CreateSimpleInteractionUI();
        }
    }
    
    [ContextMenu("Create Simple Interaction UI")]
    public void CreateSimpleInteractionUI()
    {
        Debug.Log("Creating Simple Interaction UI...");
        
        // Find or create canvas
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.Log("No Canvas found. Creating one automatically...");
                targetCanvas = CreateMainCanvas();
            }
        }
        
        // Create interaction canvas
        GameObject interactionCanvasGO = new GameObject("InteractionCanvas");
        interactionCanvasGO.transform.SetParent(targetCanvas.transform, false);
        
        Canvas interactionCanvas = interactionCanvasGO.AddComponent<Canvas>();
        interactionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        interactionCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = interactionCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        interactionCanvasGO.AddComponent<GraphicRaycaster>();
        CanvasGroup canvasGroup = interactionCanvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Start hidden
        
        // Create interaction prompt
        GameObject promptGO;
        
        if (steampunkFramePrefab != null)
        {
            promptGO = Instantiate(steampunkFramePrefab, interactionCanvasGO.transform);
            promptGO.name = "InteractionPrompt";
            Debug.Log("Using SteampunkUI Frame prefab");
        }
        else
        {
            promptGO = CreateBasicPrompt(interactionCanvasGO);
            Debug.Log("Using basic UI prompt");
        }
        
        // Setup prompt
        RectTransform promptRect = promptGO.GetComponent<RectTransform>();
        if (promptRect != null)
        {
            promptRect.sizeDelta = new Vector2(250, 80);
            promptRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.anchoredPosition = Vector2.zero;
        }
        
        // Setup text
        TextMeshProUGUI text = promptGO.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(promptGO.transform, false);
            
            text = textGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        text.text = string.Format(interactionMessage, interactionKey);
        
        // Fix font asset issue
        if (text.font == null)
        {
            // Try to find a default TextMeshPro font
            TMPro.TMP_FontAsset defaultFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                // Try alternative font paths
                defaultFont = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().FirstOrDefault();
            }
            
            if (defaultFont != null)
            {
                text.font = defaultFont;
                Debug.Log($"Assigned font: {defaultFont.name}");
            }
            else
            {
                Debug.LogWarning("No TextMeshPro font found. Text may not display correctly.");
            }
        }
        
        // Check if InteractionUI already exists to avoid duplicates
        InteractionUI existingUI = FindObjectOfType<InteractionUI>();
        if (existingUI != null)
        {
            Debug.Log("InteractionUI already exists. Destroying old one and creating new one.");
            DestroyImmediate(existingUI.gameObject);
        }
        
        // Add InteractionUI component
        InteractionUI interactionUI = interactionCanvasGO.AddComponent<InteractionUI>();
        
        // Set references using SerializeField approach BEFORE Awake is called
        SetPrivateField(interactionUI, "interactionCanvas", interactionCanvas);
        SetPrivateField(interactionUI, "interactionPrompt", promptGO);
        SetPrivateField(interactionUI, "interactionText", text);
        SetPrivateField(interactionUI, "interactionIcon", null);
        
        // Manually initialize the InteractionUI with proper references
        InitializeInteractionUI(interactionUI);
        
        Debug.Log("Simple Interaction UI created successfully!");
        Debug.Log($"Canvas: {interactionCanvas.name}");
        Debug.Log($"Prompt: {promptGO.name}");
        Debug.Log($"Text: {text.name}");
    }
    
    GameObject CreateBasicPrompt(GameObject parent)
    {
        GameObject promptGO = new GameObject("BasicPrompt");
        promptGO.transform.SetParent(parent.transform, false);
        
        Image bg = promptGO.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        return promptGO;
    }
    
    Canvas CreateMainCanvas()
    {
        // Create main canvas
        GameObject canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        
        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Add EventSystem if it doesn't exist
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Debug.Log("Main Canvas created successfully!");
        return canvas;
    }
    
    void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogError($"Field '{fieldName}' not found in {target.GetType().Name}");
        }
    }
    
    void InitializeInteractionUI(InteractionUI interactionUI)
    {
        // Manually set up the InteractionUI without relying on Awake
        CanvasGroup canvasGroup = interactionUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = interactionUI.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set initial state
        canvasGroup.alpha = 0f;
        
        // Find camera and interactor
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        Interactor interactor = FindObjectOfType<Interactor>();
        
        // Set private fields for camera and interactor
        SetPrivateField(interactionUI, "playerCamera", playerCamera);
        SetPrivateField(interactionUI, "interactor", interactor);
        SetPrivateField(interactionUI, "canvasGroup", canvasGroup);
        
        // Set original scale
        GameObject prompt = GetPrivateField<GameObject>(interactionUI, "interactionPrompt");
        if (prompt != null)
        {
            SetPrivateField(interactionUI, "originalScale", prompt.transform.localScale);
        }
        
        Debug.Log("InteractionUI manually initialized successfully!");
    }
    
    T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return (T)field.GetValue(target);
        }
        return default(T);
    }
}
