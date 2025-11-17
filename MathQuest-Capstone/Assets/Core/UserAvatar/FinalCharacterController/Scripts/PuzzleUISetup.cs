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

        // Create puzzle window
        GameObject window = new GameObject("PuzzleWindow");
        window.transform.SetParent(puzzlePanel.transform, false);

        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(400, 300);
        windowRect.anchoredPosition = Vector2.zero;

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Create title
        GameObject titleObj = CreateText("Title", window.transform, "Solve the Puzzle", 32, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(-40, 60);
        titleRect.anchoredPosition = new Vector2(0, -30);

        // Create question text
        GameObject questionObj = CreateText("QuestionText", window.transform, "2 + 2 = ?", 36, FontStyles.Bold);
        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0.5f, 0.5f);
        questionRect.anchorMax = new Vector2(0.5f, 0.5f);
        questionRect.sizeDelta = new Vector2(300, 80);
        questionRect.anchoredPosition = new Vector2(0, 40);

        // Create input field
        GameObject inputObj = CreateInputField("AnswerInput", window.transform);
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(200, 50);
        inputRect.anchoredPosition = new Vector2(0, -20);

        // Create feedback text
        GameObject feedbackObj = CreateText("FeedbackText", window.transform, "", 20, FontStyles.Normal);
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0.5f);
        feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
        feedbackRect.sizeDelta = new Vector2(300, 40);
        feedbackRect.anchoredPosition = new Vector2(0, -70);

        // Create submit button
        GameObject submitBtn = CreateButton("SubmitButton", window.transform, "Submit");
        RectTransform submitRect = submitBtn.GetComponent<RectTransform>();
        submitRect.anchorMin = new Vector2(0.5f, 0);
        submitRect.anchorMax = new Vector2(0.5f, 0);
        submitRect.sizeDelta = new Vector2(150, 40);
        submitRect.anchoredPosition = new Vector2(-80, 30);

        // Create cancel button
        GameObject cancelBtn = CreateButton("CancelButton", window.transform, "Cancel");
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.sizeDelta = new Vector2(150, 40);
        cancelRect.anchoredPosition = new Vector2(80, 30);

        // Add SimpleMathPuzzle component
        SimpleMathPuzzle puzzleScript = puzzlePanel.AddComponent<SimpleMathPuzzle>();

        // Use reflection to set private fields
        SetPrivateField(puzzleScript, "puzzlePanel", puzzlePanel);
        SetPrivateField(puzzleScript, "questionText", questionObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(puzzleScript, "answerInput", inputObj.GetComponent<TMP_InputField>());
        SetPrivateField(puzzleScript, "submitButton", submitBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "cancelButton", cancelBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "feedbackText", feedbackObj.GetComponent<TextMeshProUGUI>());

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

        GameObject window = new GameObject("PuzzleWindow");
        window.transform.SetParent(puzzlePanel.transform, false);

        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(600, 520);
        windowRect.anchoredPosition = Vector2.zero;

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        GameObject titleObj = CreateText("Title", window.transform, "Choose the Correct Answer", 30, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(-40, 60);
        titleRect.anchoredPosition = new Vector2(0, -30);

        GameObject promptTextObj = CreateText("PromptText", window.transform, "Prompt goes here", 28, FontStyles.Bold);
        RectTransform promptRect = promptTextObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.75f);
        promptRect.anchorMax = new Vector2(0.5f, 0.75f);
        promptRect.sizeDelta = new Vector2(520, 120);
        promptRect.anchoredPosition = new Vector2(0, 0);

        GameObject promptImageObj = new GameObject("PromptImage");
        promptImageObj.transform.SetParent(window.transform, false);
        RectTransform promptImageRect = promptImageObj.AddComponent<RectTransform>();
        promptImageRect.anchorMin = new Vector2(0.5f, 0.45f);
        promptImageRect.anchorMax = new Vector2(0.5f, 0.45f);
        promptImageRect.sizeDelta = new Vector2(360, 200);
        promptImageRect.anchoredPosition = new Vector2(0, 20);

        Image promptImage = promptImageObj.AddComponent<Image>();
        promptImage.color = Color.white;
        promptImage.preserveAspect = true;
        promptImage.gameObject.SetActive(false);

        GameObject optionsContainer = new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(window.transform, false);
        RectTransform optionsRect = optionsContainer.AddComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.5f, 0);
        optionsRect.anchorMax = new Vector2(0.5f, 0);
        optionsRect.sizeDelta = new Vector2(520, 220);
        optionsRect.anchoredPosition = new Vector2(0, -60);

        VerticalLayoutGroup layoutGroup = optionsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        ContentSizeFitter fitter = optionsContainer.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        const int optionCount = 4;
        Button[] optionButtons = new Button[optionCount];
        TextMeshProUGUI[] optionLabels = new TextMeshProUGUI[optionCount];

        for (int i = 0; i < optionCount; i++)
        {
            GameObject optionObj = CreateButton($"Option{i + 1}", optionsContainer.transform, $"Option {i + 1}");
            RectTransform optionRect = optionObj.GetComponent<RectTransform>();
            optionRect.sizeDelta = new Vector2(0, 50);

            LayoutElement layout = optionObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;
            layout.flexibleWidth = 1f;

            Button button = optionObj.GetComponent<Button>();
            TextMeshProUGUI label = optionObj.GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Left;
            label.margin = new Vector4(20, 10, 20, 10);

            optionButtons[i] = button;
            optionLabels[i] = label;
        }

        GameObject feedbackObj = CreateText("FeedbackText", window.transform, "", 22, FontStyles.Italic);
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0);
        feedbackRect.anchorMax = new Vector2(0.5f, 0);
        feedbackRect.sizeDelta = new Vector2(520, 40);
        feedbackRect.anchoredPosition = new Vector2(0, -220);

        GameObject cancelBtn = CreateButton("CancelButton", window.transform, "Cancel");
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0);
        cancelRect.anchorMax = new Vector2(0.5f, 0);
        cancelRect.sizeDelta = new Vector2(180, 42);
        cancelRect.anchoredPosition = new Vector2(0, -270);

        MultipleChoicePuzzle puzzleScript = puzzlePanel.AddComponent<MultipleChoicePuzzle>();
        SetPrivateField(puzzleScript, "puzzlePanel", puzzlePanel);
        SetPrivateField(puzzleScript, "promptText", promptTextObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(puzzleScript, "promptImage", promptImage);
        SetPrivateField(puzzleScript, "optionButtons", optionButtons);
        SetPrivateField(puzzleScript, "optionLabels", optionLabels);
        SetPrivateField(puzzleScript, "cancelButton", cancelBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "feedbackText", feedbackObj.GetComponent<TextMeshProUGUI>());

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

