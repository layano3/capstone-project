using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUITest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool showTestUI = true;
    public KeyCode testToggleKey = KeyCode.T;
    
    private GameObject testUI;
    private TextMeshProUGUI testText;
    
    void Start()
    {
        CreateTestUI();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testToggleKey))
        {
            showTestUI = !showTestUI;
            if (testUI != null)
            {
                testUI.SetActive(showTestUI);
            }
        }
    }
    
    void CreateTestUI()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("No Canvas found. Creating TestCanvas automatically...");
            canvas = CreateTestCanvas();
        }
        
        // Create test UI
        testUI = new GameObject("TestInteractionUI");
        testUI.transform.SetParent(canvas.transform, false);
        
        // Add background
        Image bg = testUI.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Setup rect transform
        RectTransform rect = testUI.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 100);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 200);
        
        // Add text
        GameObject textGO = new GameObject("TestText");
        textGO.transform.SetParent(testUI.transform, false);
        
        testText = textGO.AddComponent<TextMeshProUGUI>();
        testText.text = "Test UI - Press T to toggle";
        testText.fontSize = 24;
        testText.color = Color.white;
        testText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("Test UI created! Press T to toggle visibility.");
    }
    
    Canvas CreateTestCanvas()
    {
        GameObject canvasGO = new GameObject("TestCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // Higher than main canvas
        
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
        
        Debug.Log("Test Canvas created successfully!");
        return canvas;
    }
    
    public void UpdateTestText(string message)
    {
        if (testText != null)
        {
            testText.text = message;
        }
    }
}
