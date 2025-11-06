using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class InteractionUISetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("SteampunkUI Settings")]
    [SerializeField] private string interactionKey = "E";
    [SerializeField] private string interactionMessage = "Press {0} to interact";
    [SerializeField] private int fontSize = 24;
    
    [Header("SteampunkUI Prefabs")]
    [SerializeField] private GameObject steampunkButtonPrefab; // Assign Button 1.prefab or similar
    [SerializeField] private GameObject steampunkFramePrefab; // Assign Frame 1.prefab or similar
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupInteractionUI();
        }
    }
    
    [ContextMenu("Setup Interaction UI")]
    public void SetupInteractionUI()
    {
        // Find or create the main canvas
        Canvas mainCanvas = FindMainCanvas();
        if (mainCanvas == null)
        {
            Debug.LogError("InteractionUISetup: No main canvas found! Please create a Canvas first.");
            return;
        }
        
        // Create the interaction canvas
        GameObject interactionCanvasGO = CreateInteractionCanvas(mainCanvas);
        
        // Create the interaction prompt using SteampunkUI
        GameObject promptGO = CreateSteampunkInteractionPrompt(interactionCanvasGO);
        
        // Setup the InteractionUI component
        InteractionUI interactionUI = interactionCanvasGO.GetComponent<InteractionUI>();
        if (interactionUI == null)
        {
            interactionUI = interactionCanvasGO.AddComponent<InteractionUI>();
        }
        
        // Assign the UI references
        var canvas = interactionCanvasGO.GetComponent<Canvas>();
        var prompt = promptGO;
        var text = promptGO.GetComponentInChildren<TextMeshProUGUI>();
        var icon = promptGO.GetComponentInChildren<Image>();
        
        // Use reflection to set private fields (since they're serialized)
        var interactionUIFields = typeof(InteractionUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in interactionUIFields)
        {
            switch (field.Name)
            {
                case "interactionCanvas":
                    field.SetValue(interactionUI, canvas);
                    break;
                case "interactionPrompt":
                    field.SetValue(interactionUI, prompt);
                    break;
                case "interactionText":
                    field.SetValue(interactionUI, text);
                    break;
                case "interactionIcon":
                    field.SetValue(interactionUI, icon);
                    break;
            }
        }
        
        Debug.Log("SteampunkUI Interaction UI setup complete!");
    }
    
    Canvas FindMainCanvas()
    {
        // First try to find a canvas with "Main" in the name
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name.ToLower().Contains("main"))
            {
                return canvas;
            }
        }
        
        // If no main canvas, return the first one found
        if (canvases.Length > 0)
        {
            return canvases[0];
        }
        
        // If no canvas exists, create one automatically
        Debug.Log("No Canvas found in scene. Creating one automatically...");
        return CreateMainCanvas();
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
    
    GameObject CreateInteractionCanvas(Canvas parentCanvas)
    {
        GameObject canvasGO = new GameObject("InteractionCanvas");
        canvasGO.transform.SetParent(parentCanvas.transform, false);
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to appear on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<CanvasGroup>();
        
        return canvasGO;
    }
    
    GameObject CreateSteampunkInteractionPrompt(GameObject parent)
    {
        GameObject promptGO;
        
        // Try to use SteampunkUI Frame prefab if available
        if (steampunkFramePrefab != null)
        {
            promptGO = Instantiate(steampunkFramePrefab, parent.transform);
            promptGO.name = "SteampunkInteractionPrompt";
        }
        else
        {
            // Fallback to basic UI if no SteampunkUI prefab assigned
            promptGO = CreateBasicInteractionPrompt(parent);
            return promptGO;
        }
        
        // Setup RectTransform
        RectTransform rectTransform = promptGO.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(250, 80);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        // Find or create text component
        TextMeshProUGUI text = promptGO.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            // Create text if not found
            GameObject textGO = new GameObject("InteractionText");
            textGO.transform.SetParent(promptGO.transform, false);
            
            text = textGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        // Set the text content
        text.text = string.Format(interactionMessage, interactionKey);
        
        return promptGO;
    }
    
    GameObject CreateBasicInteractionPrompt(GameObject parent)
    {
        GameObject promptGO = new GameObject("BasicInteractionPrompt");
        promptGO.transform.SetParent(parent.transform, false);
        
        // Add Image component for background
        Image background = promptGO.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add RectTransform and set size
        RectTransform rectTransform = promptGO.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 60);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Create text
        GameObject textGO = new GameObject("InteractionText");
        textGO.transform.SetParent(promptGO.transform, false);
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = string.Format(interactionMessage, interactionKey);
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return promptGO;
    }
    
}
