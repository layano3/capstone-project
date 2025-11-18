using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Helper script to automatically create a puzzle UI in your scene.
/// Attach this to any GameObject and run the context menu option.
/// </summary>
public class PuzzleUISetup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool createOnStart = false;
    
    [ContextMenu("Create Math Puzzle UI")]
    public void CreateMathPuzzleUI()
    {
        Debug.Log("Creating Math Puzzle UI...");

        // Find or create main canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.Log("No Canvas found. Creating one...");
            mainCanvas = CreateMainCanvas();
        }

        // Create puzzle panel
        GameObject puzzlePanel = new GameObject("MathPuzzlePanel");
        puzzlePanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform panelRect = puzzlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Add semi-transparent background
        Image bgImage = puzzlePanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // Create responsive puzzle window
        GameObject window = new GameObject("PuzzleWindow");
        window.transform.SetParent(puzzlePanel.transform, false);

        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(450, 0); // Width fixed, height auto
        windowRect.anchoredPosition = Vector2.zero;

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Add VerticalLayoutGroup for responsive stacking
        VerticalLayoutGroup windowLayout = window.AddComponent<VerticalLayoutGroup>();
        windowLayout.spacing = 15;
        windowLayout.padding = new RectOffset(30, 30, 20, 20);
        windowLayout.childControlHeight = false;
        windowLayout.childControlWidth = true;
        windowLayout.childForceExpandHeight = false;
        windowLayout.childForceExpandWidth = true;

        // Add ContentSizeFitter so window auto-sizes
        ContentSizeFitter windowFitter = window.AddComponent<ContentSizeFitter>();
        windowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        windowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Create title with auto-sizing
        GameObject titleObj = CreateText("Title", window.transform, "Solve the Puzzle", 32, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 50;
        
        ContentSizeFitter titleFitter = titleObj.AddComponent<ContentSizeFitter>();
        titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create question text with auto-sizing and word wrapping
        GameObject questionObj = CreateText("QuestionText", window.transform, "2 + 2 = ?", 36, FontStyles.Bold);
        TextMeshProUGUI questionTMP = questionObj.GetComponent<TextMeshProUGUI>();
        questionTMP.enableWordWrapping = true;
        questionTMP.overflowMode = TextOverflowModes.Truncate;
        
        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0, 1);
        questionRect.anchorMax = new Vector2(1, 1);
        questionRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement questionLayout = questionObj.AddComponent<LayoutElement>();
        questionLayout.minHeight = 50;
        questionLayout.flexibleHeight = 0;
        
        ContentSizeFitter questionFitter = questionObj.AddComponent<ContentSizeFitter>();
        questionFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create input field
        GameObject inputObj = CreateInputField("AnswerInput", window.transform);
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        
        LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
        inputLayout.preferredWidth = 250;
        inputLayout.preferredHeight = 50;
        inputLayout.flexibleWidth = 0;
        inputLayout.flexibleHeight = 0;

        // Create feedback text with auto-sizing
        GameObject feedbackObj = CreateText("FeedbackText", window.transform, "", 20, FontStyles.Normal);
        TextMeshProUGUI feedbackTMP = feedbackObj.GetComponent<TextMeshProUGUI>();
        feedbackTMP.enableWordWrapping = true;
        
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        
        LayoutElement feedbackLayout = feedbackObj.AddComponent<LayoutElement>();
        feedbackLayout.minHeight = 30;
        feedbackLayout.flexibleHeight = 0;
        
        ContentSizeFitter feedbackFitter = feedbackObj.AddComponent<ContentSizeFitter>();
        feedbackFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create button container for horizontal layout
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(window.transform, false);
        
        HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 15;
        buttonLayout.childControlWidth = false;
        buttonLayout.childControlHeight = false;
        buttonLayout.childForceExpandWidth = false;
        buttonLayout.childForceExpandHeight = false;
        
        LayoutElement buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
        buttonContainerLayout.flexibleHeight = 0;

        // Create submit button
        GameObject submitBtn = CreateButton("SubmitButton", buttonContainer.transform, "Submit");
        LayoutElement submitLayout = submitBtn.AddComponent<LayoutElement>();
        submitLayout.preferredWidth = 150;
        submitLayout.preferredHeight = 40;
        submitLayout.flexibleWidth = 0;
        submitLayout.flexibleHeight = 0;

        // Create cancel button
        GameObject cancelBtn = CreateButton("CancelButton", buttonContainer.transform, "Cancel");
        LayoutElement cancelLayout = cancelBtn.AddComponent<LayoutElement>();
        cancelLayout.preferredWidth = 150;
        cancelLayout.preferredHeight = 40;
        cancelLayout.flexibleWidth = 0;
        cancelLayout.flexibleHeight = 0;

        // Add SimpleMathPuzzle component
        SimpleMathPuzzle puzzleScript = puzzlePanel.AddComponent<SimpleMathPuzzle>();

        // Use reflection to set private fields
        SetPrivateField(puzzleScript, "puzzlePanel", puzzlePanel);
        SetPrivateField(puzzleScript, "questionText", questionTMP);
        SetPrivateField(puzzleScript, "answerInput", inputObj.GetComponent<TMP_InputField>());
        SetPrivateField(puzzleScript, "submitButton", submitBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "cancelButton", cancelBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "feedbackText", feedbackTMP);

        puzzlePanel.SetActive(false);

        Debug.Log("Math Puzzle UI created successfully!");
        Debug.Log("Assign this SimpleMathPuzzle component to your PuzzleLockedChest!");
    }

    [ContextMenu("Create Multiple Choice Puzzle UI")]
    public void CreateMultipleChoicePuzzleUI()
    {
        Debug.Log("Creating Multiple Choice Puzzle UI...");

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.Log("No Canvas found. Creating one...");
            mainCanvas = CreateMainCanvas();
        }

        GameObject puzzlePanel = new GameObject("MultipleChoicePuzzlePanel");
        puzzlePanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform panelRect = puzzlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = puzzlePanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // Create responsive window with VerticalLayoutGroup
        GameObject window = new GameObject("PuzzleWindow");
        window.transform.SetParent(puzzlePanel.transform, false);

        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(600, 0); // Width fixed, height auto
        windowRect.anchoredPosition = Vector2.zero;

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Add VerticalLayoutGroup to window for responsive stacking
        VerticalLayoutGroup windowLayout = window.AddComponent<VerticalLayoutGroup>();
        windowLayout.spacing = 15;
        windowLayout.padding = new RectOffset(30, 30, 20, 20);
        windowLayout.childControlHeight = false;
        windowLayout.childControlWidth = true;
        windowLayout.childForceExpandHeight = false;
        windowLayout.childForceExpandWidth = true;

        // Add ContentSizeFitter to window so it auto-sizes based on content
        ContentSizeFitter windowFitter = window.AddComponent<ContentSizeFitter>();
        windowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        windowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Create title with auto-sizing
        GameObject titleObj = CreateText("Title", window.transform, "Choose the Correct Answer", 30, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 50;
        
        ContentSizeFitter titleFitter = titleObj.AddComponent<ContentSizeFitter>();
        titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create prompt text container with auto-sizing
        GameObject promptTextObj = CreateText("PromptText", window.transform, "Prompt goes here", 28, FontStyles.Bold);
        TextMeshProUGUI promptTMP = promptTextObj.GetComponent<TextMeshProUGUI>();
        promptTMP.enableWordWrapping = true;
        promptTMP.overflowMode = TextOverflowModes.Truncate;
        
        RectTransform promptRect = promptTextObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0, 1);
        promptRect.anchorMax = new Vector2(1, 1);
        promptRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement promptLayout = promptTextObj.AddComponent<LayoutElement>();
        promptLayout.minHeight = 40;
        promptLayout.flexibleHeight = 0;
        
        ContentSizeFitter promptFitter = promptTextObj.AddComponent<ContentSizeFitter>();
        promptFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create prompt image container (auto-hides when no image)
        GameObject promptImageObj = new GameObject("PromptImage");
        promptImageObj.transform.SetParent(window.transform, false);
        RectTransform promptImageRect = promptImageObj.AddComponent<RectTransform>();
        promptImageRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptImageRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptImageRect.pivot = new Vector2(0.5f, 0.5f);

        Image promptImage = promptImageObj.AddComponent<Image>();
        promptImage.color = Color.white;
        promptImage.preserveAspect = true;
        promptImage.gameObject.SetActive(false);

        LayoutElement imageLayout = promptImageObj.AddComponent<LayoutElement>();
        imageLayout.preferredWidth = 540;
        imageLayout.preferredHeight = 300;
        imageLayout.flexibleWidth = 0;
        imageLayout.flexibleHeight = 0;
        imageLayout.ignoreLayout = false; // Will be toggled based on image presence

        // Create options container with auto-sizing
        GameObject optionsContainer = new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(window.transform, false);
        RectTransform optionsRect = optionsContainer.AddComponent<RectTransform>();

        VerticalLayoutGroup optionsLayout = optionsContainer.AddComponent<VerticalLayoutGroup>();
        optionsLayout.spacing = 10;
        optionsLayout.childControlHeight = false;
        optionsLayout.childControlWidth = true;
        optionsLayout.childForceExpandHeight = false;
        optionsLayout.childForceExpandWidth = true;

        ContentSizeFitter optionsFitter = optionsContainer.AddComponent<ContentSizeFitter>();
        optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement optionsLayoutElement = optionsContainer.AddComponent<LayoutElement>();
        optionsLayoutElement.flexibleHeight = 0;

        const int optionCount = 4;
        Button[] optionButtons = new Button[optionCount];
        TextMeshProUGUI[] optionLabels = new TextMeshProUGUI[optionCount];

        for (int i = 0; i < optionCount; i++)
        {
            GameObject optionObj = CreateButton($"Option{i + 1}", optionsContainer.transform, $"Option {i + 1}");
            Button button = optionObj.GetComponent<Button>();
            TextMeshProUGUI label = optionObj.GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Left;
            label.margin = new Vector4(20, 10, 20, 10);
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Truncate;

            LayoutElement optionLayout = optionObj.AddComponent<LayoutElement>();
            optionLayout.minHeight = 50;
            optionLayout.flexibleHeight = 0;
            optionLayout.flexibleWidth = 1f;

            ContentSizeFitter optionFitter = optionObj.AddComponent<ContentSizeFitter>();
            optionFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            optionButtons[i] = button;
            optionLabels[i] = label;
        }

        // Create feedback text with auto-sizing
        GameObject feedbackObj = CreateText("FeedbackText", window.transform, "", 22, FontStyles.Italic);
        TextMeshProUGUI feedbackTMP = feedbackObj.GetComponent<TextMeshProUGUI>();
        feedbackTMP.enableWordWrapping = true;
        
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        
        LayoutElement feedbackLayout = feedbackObj.AddComponent<LayoutElement>();
        feedbackLayout.minHeight = 30;
        feedbackLayout.flexibleHeight = 0;
        
        ContentSizeFitter feedbackFitter = feedbackObj.AddComponent<ContentSizeFitter>();
        feedbackFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create cancel button
        GameObject cancelBtn = CreateButton("CancelButton", window.transform, "Cancel");
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        
        LayoutElement cancelLayout = cancelBtn.AddComponent<LayoutElement>();
        cancelLayout.preferredWidth = 180;
        cancelLayout.preferredHeight = 42;
        cancelLayout.flexibleWidth = 0;
        cancelLayout.flexibleHeight = 0;

        MultipleChoicePuzzle puzzleScript = puzzlePanel.AddComponent<MultipleChoicePuzzle>();
        SetPrivateField(puzzleScript, "puzzlePanel", puzzlePanel);
        SetPrivateField(puzzleScript, "promptText", promptTMP);
        SetPrivateField(puzzleScript, "promptImage", promptImage);
        SetPrivateField(puzzleScript, "optionButtons", optionButtons);
        SetPrivateField(puzzleScript, "optionLabels", optionLabels);
        SetPrivateField(puzzleScript, "cancelButton", cancelBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "feedbackText", feedbackTMP);

        var sampleQuestion = new MultipleChoicePuzzle.MultipleChoiceQuestion
        {
            prompt = "Quel est le nombre qui continue cette suite : 2, 5, 10, 17, 26, ... ?",
            promptImage = null,
            options = new[]
            {
                new MultipleChoicePuzzle.ChoiceOption { label = "A) 30", isCorrect = false },
                new MultipleChoicePuzzle.ChoiceOption { label = "B) 34", isCorrect = false },
                new MultipleChoicePuzzle.ChoiceOption { label = "C) 35", isCorrect = false },
                new MultipleChoicePuzzle.ChoiceOption { label = "D) 37", isCorrect = true },
            }
        };

        SetPrivateField(puzzleScript, "questions", new[] { sampleQuestion });

        puzzlePanel.SetActive(false);

        Debug.Log("Multiple Choice Puzzle UI created successfully!");
        Debug.Log("Assign this MultipleChoicePuzzle component to any PuzzleLockedInteractable to gate content.");
    }

    private GameObject CreateText(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Try to assign a font
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
            font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
        if (font != null)
            tmp.font = font;

        return textObj;
    }

    private GameObject CreateInputField(string name, Transform parent)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);

        Image bgImage = inputObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // Create text area
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);

        // Create placeholder
        GameObject placeholder = CreateText("Placeholder", textArea.transform, "Enter answer...", 24, FontStyles.Italic);
        TextMeshProUGUI placeholderTMP = placeholder.GetComponent<TextMeshProUGUI>();
        placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        // Create text
        GameObject textObj = CreateText("Text", textArea.transform, "", 24, FontStyles.Normal);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        inputField.textViewport = textAreaRect;
        inputField.textComponent = textObj.GetComponent<TextMeshProUGUI>();
        inputField.placeholder = placeholderTMP;

        return inputObj;
    }

    private GameObject CreateButton(string name, Transform parent, string buttonText)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;

        // Create button text
        GameObject textObj = CreateText("Text", btnObj.transform, buttonText, 20, FontStyles.Bold);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    private Canvas CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Add EventSystem if needed
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvas;
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }

    void Start()
    {
        if (createOnStart)
        {
            CreateMathPuzzleUI();
        }
    }
}

