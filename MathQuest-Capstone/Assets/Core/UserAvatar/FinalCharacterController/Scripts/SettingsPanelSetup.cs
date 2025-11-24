using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Automatically creates the Settings Panel UI with sensitivity and SFX volume controls.
/// Attach this to any GameObject and use the context menu "Create Settings Panel" or enable createOnStart.
/// </summary>
public class SettingsPanelSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool createOnStart = false;
    
    [Header("Target Canvas (Optional)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Prefabs (Optional)")]
    [Tooltip("If assigned, this slider prefab will be used instead of creating sliders programmatically")]
    [SerializeField] private GameObject sliderPrefab;

    [Header("Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.85f);
    [SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private float spacing = 30f;

    private void Start()
    {
        if (createOnStart)
        {
            CreateSettingsPanel();
        }
    }

    [ContextMenu("Create Settings Panel")]
    public void CreateSettingsPanel()
    {
        Debug.Log("Creating Settings Panel UI...");

        // Check if SettingsPanel already exists
        SettingsPanel existingPanel = FindObjectOfType<SettingsPanel>();
        if (existingPanel != null)
        {
            Debug.LogWarning("SettingsPanel already exists. Destroying old one and creating new one.");
            DestroyImmediate(existingPanel.gameObject);
        }

        // Find or create canvas
        Canvas canvas = targetCanvas;
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("No Canvas found. Creating one automatically...");
                canvas = CreateMainCanvas();
            }
        }

        // Create SettingsPanel GameObject
        GameObject settingsPanelGO = new GameObject("SettingsPanel");
        settingsPanelGO.transform.SetParent(canvas.transform, false);

        // Add SettingsPanel component
        SettingsPanel settingsPanelComponent = settingsPanelGO.AddComponent<SettingsPanel>();

        // Create background panel
        GameObject backgroundPanel = CreateBackgroundPanel(settingsPanelGO);
        
        // Create settings container
        GameObject settingsContainer = CreateSettingsContainer(backgroundPanel);
        
        // Create title
        GameObject titleText = CreateTitle(settingsContainer);
        
        // Create sensitivity controls
        CreateSensitivityControls(settingsContainer, out Slider sensitivityHSlider, out Slider sensitivityVSlider,
            out TMP_Text sensitivityHValueText, out TMP_Text sensitivityVValueText);
        
        // Create SFX volume control
        CreateSFXVolumeControl(settingsContainer, out Slider sfxVolumeSlider, out TMP_Text sfxVolumeValueText);
        
        // Create close button
        GameObject closeButton = CreateCloseButton(settingsContainer);

        // Assign all references to SettingsPanel component using reflection
        AssignReferencesToComponent(settingsPanelComponent, backgroundPanel, sensitivityHSlider, 
            sensitivityVSlider, sfxVolumeSlider, sensitivityHValueText, sensitivityVValueText, 
            sfxVolumeValueText, closeButton.GetComponent<Button>());

        Debug.Log("Settings Panel created successfully!");
        Debug.Log($"SettingsPanel GameObject: {settingsPanelGO.name}");
        Debug.Log($"Background Panel: {backgroundPanel.name}");
        Debug.Log($"Settings Container: {settingsContainer.name}");
    }

    private GameObject CreateBackgroundPanel(GameObject parent)
    {
        GameObject panel = new GameObject("SettingsPanel_BG");
        panel.transform.SetParent(parent.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = backgroundColor;

        return panel;
    }

    private GameObject CreateSettingsContainer(GameObject parent)
    {
        GameObject container = new GameObject("SettingsContainer");
        container.transform.SetParent(parent.transform, false);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(550, 500);
        rect.anchoredPosition = Vector2.zero;

        Image image = container.AddComponent<Image>();
        image.color = panelColor;

        // Add Vertical Layout Group
        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        // Add Content Size Fitter
        ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        return container;
    }

    private GameObject CreateTitle(GameObject parent)
    {
        GameObject title = new GameObject("TitleText");
        title.transform.SetParent(parent.transform, false);

        RectTransform rect = title.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 50);

        TextMeshProUGUI text = title.AddComponent<TextMeshProUGUI>();
        text.text = "SETTINGS";
        text.fontSize = 42;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;

        AssignFont(text);

        LayoutElement layoutElement = title.AddComponent<LayoutElement>();
        layoutElement.minHeight = 50;
        layoutElement.flexibleHeight = 0;

        return title;
    }

    private void CreateSensitivityControls(GameObject parent, out Slider sensitivityHSlider, 
        out Slider sensitivityVSlider, out TMP_Text sensitivityHValueText, out TMP_Text sensitivityVValueText)
    {
        // Horizontal Sensitivity
        GameObject hSection = CreateSettingRow(parent, "Horizontal Sensitivity", out sensitivityHSlider, out sensitivityHValueText);
        
        // Add spacing
        CreateSpacer(parent, 10f);
        
        // Vertical Sensitivity
        GameObject vSection = CreateSettingRow(parent, "Vertical Sensitivity", out sensitivityVSlider, out sensitivityVValueText);
    }

    private void CreateSFXVolumeControl(GameObject parent, out Slider sfxVolumeSlider, out TMP_Text sfxVolumeValueText)
    {
        // Add spacing
        CreateSpacer(parent, 10f);
        
        // SFX Volume
        CreateSettingRow(parent, "SFX Volume", out sfxVolumeSlider, out sfxVolumeValueText);
    }

    private GameObject CreateSettingRow(GameObject parent, string labelText, out Slider slider, out TMP_Text valueText)
    {
        GameObject row = new GameObject($"{labelText}Row");
        row.transform.SetParent(parent.transform, false);

        RectTransform rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.childControlWidth = false;
        layout.childControlHeight = false; // Changed to false to respect child sizes
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement layoutElement = row.AddComponent<LayoutElement>();
        layoutElement.minHeight = 80;
        layoutElement.flexibleHeight = 0;

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(row.transform, false);
        TextMeshProUGUI labelTextComponent = label.AddComponent<TextMeshProUGUI>();
        labelTextComponent.text = labelText + ":";
        labelTextComponent.fontSize = fontSize;
        labelTextComponent.alignment = TextAlignmentOptions.Left;
        labelTextComponent.color = Color.white;
        AssignFont(labelTextComponent);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(200, 0);

        LayoutElement labelLayout = label.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 200;
        labelLayout.flexibleWidth = 0;

        // Slider - Use prefab if provided, otherwise create programmatically
        GameObject sliderGO;
        if (sliderPrefab != null)
        {
            sliderGO = Instantiate(sliderPrefab, row.transform);
            sliderGO.name = "Slider";
            
            // Reset transform to ensure proper positioning
            RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                sliderRect.localScale = Vector3.one;
                sliderRect.localRotation = Quaternion.identity;
                sliderRect.localPosition = Vector3.zero;
                
                // Configure RectTransform for layout
                sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
                sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
                sliderRect.pivot = new Vector2(0.5f, 0.5f);
                sliderRect.sizeDelta = new Vector2(250, 30);
                sliderRect.anchoredPosition = Vector2.zero;
            }
            
            slider = sliderGO.GetComponent<Slider>();
            
            if (slider == null)
            {
                Debug.LogWarning("SettingsPanelSetup: Slider prefab doesn't have a Slider component. Creating default slider instead.");
                DestroyImmediate(sliderGO);
                sliderGO = CreateDefaultSlider(row.transform);
                slider = sliderGO.GetComponent<Slider>();
            }
            else
            {
                // Configure the prefab slider values
                slider.minValue = 0.1f; // Will be multiplied by 10 for display
                slider.maxValue = 10f;
                slider.value = 1f; // Default 0.1 sensitivity = 1.0 on slider
            }
        }
        else
        {
            sliderGO = CreateDefaultSlider(row.transform);
            slider = sliderGO.GetComponent<Slider>();
        }

        // Ensure LayoutElement for proper sizing
        LayoutElement sliderLayout = sliderGO.GetComponent<LayoutElement>();
        if (sliderLayout == null)
        {
            sliderLayout = sliderGO.AddComponent<LayoutElement>();
        }
        sliderLayout.preferredWidth = 250;
        sliderLayout.preferredHeight = 30;
        sliderLayout.minWidth = 250;
        sliderLayout.minHeight = 30;
        sliderLayout.flexibleWidth = 0;
        sliderLayout.flexibleHeight = 0;
        
        // Ensure slider is active and visible
        sliderGO.SetActive(true);
        
        // Debug log
        if (slider == null)
        {
            Debug.LogError($"SettingsPanelSetup: Failed to get Slider component from {sliderGO.name}");
        }
        else
        {
            Debug.Log($"SettingsPanelSetup: Created slider '{sliderGO.name}' with size {sliderGO.GetComponent<RectTransform>().sizeDelta}");
        }

        // Value Text (for SFX Volume, this will show percentage)
        GameObject valueTextGO = new GameObject("ValueText");
        valueTextGO.transform.SetParent(row.transform, false);

        RectTransform valueRect = valueTextGO.AddComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(60, 0);

        valueText = valueTextGO.AddComponent<TextMeshProUGUI>();
        valueText.text = "1.0";
        valueText.fontSize = 20;
        valueText.alignment = TextAlignmentOptions.Left;
        valueText.color = Color.white;
        AssignFont(valueText);

        LayoutElement valueLayout = valueTextGO.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 60;
        valueLayout.flexibleWidth = 0;

        return row;
    }

    private GameObject CreateDefaultSlider(Transform parent)
    {
        GameObject sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(250, 30);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0.1f; // Will be multiplied by 10 for display
        slider.maxValue = 10f;
        slider.value = 1f; // Default 0.1 sensitivity = 1.0 on slider

        // Slider Background
        GameObject sliderBG = new GameObject("Background");
        sliderBG.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = sliderBG.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        Image bgImage = sliderBG.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        slider.targetGraphic = bgImage;

        // Slider Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;

        // Slider Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        slider.fillRect = fillRect;

        // Slider Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = Vector2.zero;
        handleAreaRect.anchoredPosition = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        slider.handleRect = handleRect;

        return sliderGO;
    }

    private GameObject CreateSpacer(GameObject parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent.transform, false);

        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.flexibleHeight = 0;

        return spacer;
    }

    private GameObject CreateCloseButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("CloseButton");
        buttonGO.transform.SetParent(parent.transform, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 50);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        
        // Button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        button.colors = colors;

        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Close";
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        AssignFont(text);

        LayoutElement layoutElement = buttonGO.AddComponent<LayoutElement>();
        layoutElement.minHeight = 50;
        layoutElement.flexibleHeight = 0;
        layoutElement.minWidth = 150;

        return buttonGO;
    }

    private void AssignReferencesToComponent(SettingsPanel component, GameObject backgroundPanel,
        Slider sensitivityHSlider, Slider sensitivityVSlider, Slider sfxVolumeSlider,
        TMP_Text sensitivityHValueText, TMP_Text sensitivityVValueText, TMP_Text sfxVolumeValueText,
        Button closeButton)
    {
        // Use reflection to assign private fields
        System.Reflection.FieldInfo settingsPanelField = typeof(SettingsPanel).GetField("settingsPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (settingsPanelField != null)
        {
            settingsPanelField.SetValue(component, backgroundPanel);
        }

        System.Reflection.FieldInfo sensitivityHField = typeof(SettingsPanel).GetField("sensitivityHorizontalSlider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sensitivityHField != null)
        {
            sensitivityHField.SetValue(component, sensitivityHSlider);
        }

        System.Reflection.FieldInfo sensitivityVField = typeof(SettingsPanel).GetField("sensitivityVerticalSlider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sensitivityVField != null)
        {
            sensitivityVField.SetValue(component, sensitivityVSlider);
        }

        System.Reflection.FieldInfo sfxVolumeField = typeof(SettingsPanel).GetField("sfxVolumeSlider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sfxVolumeField != null)
        {
            sfxVolumeField.SetValue(component, sfxVolumeSlider);
        }

        System.Reflection.FieldInfo sensitivityHValueField = typeof(SettingsPanel).GetField("sensitivityHValueText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sensitivityHValueField != null)
        {
            sensitivityHValueField.SetValue(component, sensitivityHValueText);
        }

        System.Reflection.FieldInfo sensitivityVValueField = typeof(SettingsPanel).GetField("sensitivityVValueText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sensitivityVValueField != null)
        {
            sensitivityVValueField.SetValue(component, sensitivityVValueText);
        }

        System.Reflection.FieldInfo sfxVolumeValueField = typeof(SettingsPanel).GetField("sfxVolumeValueText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sfxVolumeValueField != null)
        {
            sfxVolumeValueField.SetValue(component, sfxVolumeValueText);
        }

        System.Reflection.FieldInfo closeButtonField = typeof(SettingsPanel).GetField("closeButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (closeButtonField != null)
        {
            closeButtonField.SetValue(component, closeButton);
        }
    }

    private void AssignFont(TMP_Text textComponent)
    {
        if (textComponent.font == null)
        {
            // Try to find a default TextMeshPro font
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                defaultFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
            }

            if (defaultFont != null)
            {
                textComponent.font = defaultFont;
            }
            else
            {
                Debug.LogWarning($"No TextMeshPro font found for {textComponent.name}. Text may not display correctly.");
            }
        }
    }

    private Canvas CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

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
}

