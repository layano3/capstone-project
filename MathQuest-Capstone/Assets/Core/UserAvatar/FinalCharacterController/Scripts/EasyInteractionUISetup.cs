using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class EasyInteractionUISetup : MonoBehaviour
{
    [Header("Setup")]
    public bool createOnStart = true;
    
    [Header("SteampunkUI Prefab")]
    public GameObject steampunkFramePrefab;
    
    [Header("Settings")]
    public string interactionKey = "E";
    public string interactionMessage = "Press {0} to interact";
    
    void Start()
    {
        if (createOnStart)
        {
            CreateInteractionUI();
        }
    }
    
    [ContextMenu("Create Interaction UI")]
    public void CreateInteractionUI()
    {
        Debug.Log("Creating Easy Interaction UI...");
        
        // Clean up any existing interaction UI
        SimpleInteractionUI existingUI = FindObjectOfType<SimpleInteractionUI>();
        if (existingUI != null)
        {
            Debug.Log("Destroying existing SimpleInteractionUI");
            DestroyImmediate(existingUI.gameObject);
        }
        
        // Find or create canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.Log("Creating main canvas...");
            mainCanvas = CreateMainCanvas();
        }
        
        // Create interaction canvas
        GameObject interactionCanvasGO = new GameObject("SimpleInteractionCanvas");
        interactionCanvasGO.transform.SetParent(mainCanvas.transform, false);
        
        Canvas interactionCanvas = interactionCanvasGO.AddComponent<Canvas>();
        interactionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        interactionCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = interactionCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        interactionCanvasGO.AddComponent<GraphicRaycaster>();
        CanvasGroup canvasGroup = interactionCanvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
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
            TMPro.TMP_FontAsset defaultFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                defaultFont = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().FirstOrDefault();
            }
            
            if (defaultFont != null)
            {
                text.font = defaultFont;
                Debug.Log($"Assigned font: {defaultFont.name}");
            }
        }
        
        // Add SimpleInteractionUI component
        SimpleInteractionUI interactionUI = interactionCanvasGO.AddComponent<SimpleInteractionUI>();
        
        // Set the public references directly
        interactionUI.interactionCanvas = interactionCanvas;
        interactionUI.interactionPrompt = promptGO;
        interactionUI.interactionText = text;
        interactionUI.interactionKey = interactionKey;
        interactionUI.interactionMessage = interactionMessage;
        
        Debug.Log("Easy Interaction UI created successfully!");
        Debug.Log($"Canvas: {interactionCanvas.name}");
        Debug.Log($"Prompt: {promptGO.name}");
        Debug.Log($"Text: {text.name}");
    }
    
    Canvas CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
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
    
    GameObject CreateBasicPrompt(GameObject parent)
    {
        GameObject promptGO = new GameObject("BasicPrompt");
        promptGO.transform.SetParent(parent.transform, false);
        
        Image bg = promptGO.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        return promptGO;
    }
}
