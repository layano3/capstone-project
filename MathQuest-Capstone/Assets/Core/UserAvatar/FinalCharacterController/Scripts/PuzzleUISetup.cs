using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Reflection;

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
            // Use a neutral blue-gray color for option buttons
            GameObject optionObj = CreateButton($"Option{i + 1}", optionsContainer.transform, $"Option {i + 1}", new Color(0.25f, 0.4f, 0.6f, 1f));
            Button button = optionObj.GetComponent<Button>();
            TextMeshProUGUI label = optionObj.GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Left;
            label.margin = new Vector4(20, 10, 20, 10);
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Truncate;

            LayoutElement optionLayout = optionObj.AddComponent<LayoutElement>();
            optionLayout.minHeight = 55;
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

        // Create cancel button (red for cancel action)
        GameObject cancelBtn = CreateButton("CancelButton", window.transform, "Cancel", new Color(0.7f, 0.2f, 0.2f, 1f));
        RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
        
        LayoutElement cancelLayout = cancelBtn.AddComponent<LayoutElement>();
        cancelLayout.preferredWidth = 180;
        cancelLayout.preferredHeight = 45;
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

    [ContextMenu("Fix Existing Dropdown Puzzle UI")]
    public void FixExistingDropdownPuzzleUI()
    {
        // Find existing dropdown puzzle panel
        GameObject puzzlePanel = GameObject.Find("DropdownPuzzlePanel");
        if (puzzlePanel == null)
        {
            Debug.LogError("No DropdownPuzzlePanel found! Please create one first or ensure it exists in the scene.");
            return;
        }

        Debug.Log("Fixing existing Dropdown Puzzle UI...");

        // Find the window
        Transform windowTransform = puzzlePanel.transform.Find("PuzzleWindow");
        if (windowTransform == null)
        {
            Debug.LogError("PuzzleWindow not found in DropdownPuzzlePanel!");
            return;
        }

        GameObject window = windowTransform.gameObject;

        // Fix window layout spacing and padding
        VerticalLayoutGroup windowLayout = window.GetComponent<VerticalLayoutGroup>();
        if (windowLayout != null)
        {
            windowLayout.spacing = 25;
            windowLayout.padding = new RectOffset(40, 40, 30, 30);
        }

        // Fix prompt text
        Transform promptTextTransform = window.transform.Find("PromptText");
        if (promptTextTransform != null)
        {
            TextMeshProUGUI promptTMP = promptTextTransform.GetComponent<TextMeshProUGUI>();
            if (promptTMP != null)
            {
                promptTMP.enableWordWrapping = true;
                promptTMP.overflowMode = TextOverflowModes.Overflow;
                promptTMP.fontSize = 24;
            }

            LayoutElement promptLayout = promptTextTransform.GetComponent<LayoutElement>();
            if (promptLayout != null)
            {
                promptLayout.minHeight = 50;
            }
        }

        // Fix dropdowns container
        Transform dropdownsContainerTransform = window.transform.Find("DropdownsContainer");
        if (dropdownsContainerTransform != null)
        {
            VerticalLayoutGroup dropdownsLayout = dropdownsContainerTransform.GetComponent<VerticalLayoutGroup>();
            if (dropdownsLayout != null)
            {
                dropdownsLayout.spacing = 20;
                dropdownsLayout.padding = new RectOffset(0, 0, 5, 5);
            }
        }

        // Fix each dropdown's template content layout
        DropdownPuzzle puzzleScript = puzzlePanel.GetComponent<DropdownPuzzle>();
        if (puzzleScript != null)
        {
            // Use reflection to get dropdown menus
            var dropdownMenusField = typeof(DropdownPuzzle).GetField("dropdownMenus", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (dropdownMenusField != null)
            {
                TMP_Dropdown[] dropdownMenus = dropdownMenusField.GetValue(puzzleScript) as TMP_Dropdown[];
                
                if (dropdownMenus != null)
                {
                    foreach (var dropdown in dropdownMenus)
                    {
                        if (dropdown == null || dropdown.template == null)
                            continue;

                        // Find Content in the template
                        Transform contentTransform = dropdown.template.Find("Viewport/Content");
                        if (contentTransform != null)
                        {
                            VerticalLayoutGroup contentLayout = contentTransform.GetComponent<VerticalLayoutGroup>();
                            if (contentLayout != null)
                            {
                                contentLayout.spacing = 2;
                                contentLayout.padding = new RectOffset(2, 2, 2, 2);
                            }

                            // Fix each item in the content
                            for (int i = 0; i < contentTransform.childCount; i++)
                            {
                                Transform itemTransform = contentTransform.GetChild(i);
                                if (itemTransform.name == "Item")
                                {
                                    // Update item height
                                    RectTransform itemRect = itemTransform.GetComponent<RectTransform>();
                                    if (itemRect != null)
                                    {
                                        itemRect.sizeDelta = new Vector2(0, 45);
                                    }

                                    LayoutElement itemLayout = itemTransform.GetComponent<LayoutElement>();
                                    if (itemLayout == null)
                                        itemLayout = itemTransform.gameObject.AddComponent<LayoutElement>();
                                    
                                    itemLayout.minHeight = 45;
                                    itemLayout.preferredHeight = 45;
                                    itemLayout.flexibleHeight = 0;

                                    // Fix item label
                                    Transform labelTransform = itemTransform.Find("Item Label");
                                    if (labelTransform != null)
                                    {
                                        TextMeshProUGUI labelTMP = labelTransform.GetComponent<TextMeshProUGUI>();
                                        if (labelTMP != null)
                                        {
                                            labelTMP.enableWordWrapping = true;
                                            labelTMP.overflowMode = TextOverflowModes.Overflow;
                                        }

                                        RectTransform labelRect = labelTransform.GetComponent<RectTransform>();
                                        if (labelRect != null)
                                        {
                                            labelRect.offsetMin = new Vector2(35, 5);
                                            labelRect.offsetMax = new Vector2(-10, -5);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("Dropdown Puzzle UI fixed successfully!");
    }

    [ContextMenu("Create Dropdown Puzzle UI")]
    public void CreateDropdownPuzzleUI()
    {
        Debug.Log("Creating Dropdown Puzzle UI...");

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.Log("No Canvas found. Creating one...");
            mainCanvas = CreateMainCanvas();
        }

        GameObject puzzlePanel = new GameObject("DropdownPuzzlePanel");
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
        windowRect.sizeDelta = new Vector2(650, 0); // Width fixed, height auto
        windowRect.anchoredPosition = Vector2.zero;

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Add VerticalLayoutGroup to window for responsive stacking
        VerticalLayoutGroup windowLayout = window.AddComponent<VerticalLayoutGroup>();
        windowLayout.spacing = 25; // Increased spacing between elements
        windowLayout.padding = new RectOffset(40, 40, 30, 30); // Increased padding
        windowLayout.childControlHeight = false;
        windowLayout.childControlWidth = true;
        windowLayout.childForceExpandHeight = false;
        windowLayout.childForceExpandWidth = true;

        // Add ContentSizeFitter to window so it auto-sizes based on content
        ContentSizeFitter windowFitter = window.AddComponent<ContentSizeFitter>();
        windowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        windowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Create title with auto-sizing
        GameObject titleObj = CreateText("Title", window.transform, "Select Your Answer", 32, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 55;
        
        ContentSizeFitter titleFitter = titleObj.AddComponent<ContentSizeFitter>();
        titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create prompt text container with auto-sizing
        GameObject promptTextObj = CreateText("PromptText", window.transform, "Prompt goes here", 24, FontStyles.Bold);
        TextMeshProUGUI promptTMP = promptTextObj.GetComponent<TextMeshProUGUI>();
        promptTMP.enableWordWrapping = true;
        promptTMP.overflowMode = TextOverflowModes.Overflow; // Allow text to expand
        promptTMP.autoSizeTextContainer = false; // We'll use ContentSizeFitter instead
        
        RectTransform promptRect = promptTextObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0, 1);
        promptRect.anchorMax = new Vector2(1, 1);
        promptRect.pivot = new Vector2(0.5f, 1);
        
        LayoutElement promptLayout = promptTextObj.AddComponent<LayoutElement>();
        promptLayout.minHeight = 50;
        promptLayout.flexibleHeight = 0;
        promptLayout.preferredWidth = -1; // Use full width
        
        ContentSizeFitter promptFitter = promptTextObj.AddComponent<ContentSizeFitter>();
        promptFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        promptFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

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
        imageLayout.preferredWidth = 580;
        imageLayout.preferredHeight = 350;
        imageLayout.flexibleWidth = 0;
        imageLayout.flexibleHeight = 0;
        imageLayout.ignoreLayout = false;

        // Create dropdowns container with auto-sizing
        GameObject dropdownsContainer = new GameObject("DropdownsContainer");
        dropdownsContainer.transform.SetParent(window.transform, false);
        
        VerticalLayoutGroup dropdownsLayout = dropdownsContainer.AddComponent<VerticalLayoutGroup>();
        dropdownsLayout.spacing = 20; // Increased spacing between dropdowns
        dropdownsLayout.padding = new RectOffset(0, 0, 5, 5); // Add padding
        dropdownsLayout.childControlHeight = false;
        dropdownsLayout.childControlWidth = true;
        dropdownsLayout.childForceExpandHeight = false;
        dropdownsLayout.childForceExpandWidth = true;

        LayoutElement dropdownsLayoutElement = dropdownsContainer.AddComponent<LayoutElement>();
        dropdownsLayoutElement.flexibleHeight = 0;

        ContentSizeFitter dropdownsFitter = dropdownsContainer.AddComponent<ContentSizeFitter>();
        dropdownsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        const int dropdownCount = 2; // Support up to 2 dropdowns (e.g., Name + Function)
        TMP_Dropdown[] dropdownMenus = new TMP_Dropdown[dropdownCount];
        TextMeshProUGUI[] dropdownLabels = new TextMeshProUGUI[dropdownCount];

        for (int i = 0; i < dropdownCount; i++)
        {
            GameObject dropdownContainerObj = CreateDropdown($"Dropdown{i + 1}", dropdownsContainer.transform, $"Label {i + 1}:");
            TMP_Dropdown dropdown = dropdownContainerObj.GetComponentInChildren<TMP_Dropdown>();
            
            // Get the label from the container (first child is the label)
            TextMeshProUGUI label = dropdownContainerObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            
            LayoutElement dropdownLayout = dropdown.GetComponent<LayoutElement>();
            if (dropdownLayout == null)
                dropdownLayout = dropdown.gameObject.AddComponent<LayoutElement>();
            dropdownLayout.minHeight = 45;
            dropdownLayout.flexibleHeight = 0;

            dropdownMenus[i] = dropdown;
            dropdownLabels[i] = label;
        }

        // Create feedback text with auto-sizing
        GameObject feedbackObj = CreateText("FeedbackText", window.transform, "", 22, FontStyles.Italic);
        TextMeshProUGUI feedbackTMP = feedbackObj.GetComponent<TextMeshProUGUI>();
        feedbackTMP.enableWordWrapping = true;
        
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        
        LayoutElement feedbackLayout = feedbackObj.AddComponent<LayoutElement>();
        feedbackLayout.minHeight = 35;
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

        // Create submit button (green for positive action)
        GameObject submitBtn = CreateButton("SubmitButton", buttonContainer.transform, "Submit", new Color(0.2f, 0.7f, 0.3f, 1f));
        LayoutElement submitLayout = submitBtn.AddComponent<LayoutElement>();
        submitLayout.preferredWidth = 180;
        submitLayout.preferredHeight = 45;
        submitLayout.flexibleWidth = 0;
        submitLayout.flexibleHeight = 0;

        // Create cancel button (red for cancel action)
        GameObject cancelBtn = CreateButton("CancelButton", buttonContainer.transform, "Cancel", new Color(0.7f, 0.2f, 0.2f, 1f));
        LayoutElement cancelLayout = cancelBtn.AddComponent<LayoutElement>();
        cancelLayout.preferredWidth = 180;
        cancelLayout.preferredHeight = 45;
        cancelLayout.flexibleWidth = 0;
        cancelLayout.flexibleHeight = 0;

        DropdownPuzzle puzzleScript = puzzlePanel.AddComponent<DropdownPuzzle>();
        SetPrivateField(puzzleScript, "puzzlePanel", puzzlePanel);
        SetPrivateField(puzzleScript, "promptText", promptTMP);
        SetPrivateField(puzzleScript, "promptImage", promptImage);
        SetPrivateField(puzzleScript, "dropdownMenus", dropdownMenus);
        SetPrivateField(puzzleScript, "dropdownLabels", dropdownLabels);
        SetPrivateField(puzzleScript, "submitButton", submitBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "cancelButton", cancelBtn.GetComponent<Button>());
        SetPrivateField(puzzleScript, "feedbackText", feedbackTMP);

        var sampleQuestion = new DropdownPuzzle.DropdownQuestion
        {
            prompt = "Quelle est le nom et la fonction de cette organite? (What is the name and function of this organelle?)",
            promptImage = null,
            dropdownGroups = new[]
            {
                new DropdownPuzzle.DropdownGroup
                {
                    label = "Nom:",
                    options = new[]
                    {
                        new DropdownPuzzle.ChoiceOption { label = "Mitochondrie" },
                        new DropdownPuzzle.ChoiceOption { label = "Noyau" },
                        new DropdownPuzzle.ChoiceOption { label = "Chloroplaste" },
                    },
                    correctOptionIndex = 0
                },
                new DropdownPuzzle.DropdownGroup
                {
                    label = "Fonction:",
                    options = new[]
                    {
                        new DropdownPuzzle.ChoiceOption { label = "Production d'énergie" },
                        new DropdownPuzzle.ChoiceOption { label = "Contrôle cellulaire" },
                        new DropdownPuzzle.ChoiceOption { label = "Photosynthèse" },
                    },
                    correctOptionIndex = 0
                }
            }
        };

        SetPrivateField(puzzleScript, "questions", new[] { sampleQuestion });

        puzzlePanel.SetActive(false);

        Debug.Log("Dropdown Puzzle UI created successfully!");
        Debug.Log("Assign this DropdownPuzzle component to any PuzzleLockedInteractable to gate content.");
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

    private GameObject CreateButton(string name, Transform parent, string buttonText, Color? buttonColor = null)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = buttonColor ?? new Color(0.3f, 0.5f, 0.8f, 1f);

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;
        
        // Add color transitions for better UX
        ColorBlock colors = button.colors;
        colors.normalColor = btnImage.color;
        colors.highlightedColor = new Color(btnImage.color.r * 1.2f, btnImage.color.g * 1.2f, btnImage.color.b * 1.2f, 1f);
        colors.pressedColor = new Color(btnImage.color.r * 0.8f, btnImage.color.g * 0.8f, btnImage.color.b * 0.8f, 1f);
        colors.selectedColor = btnImage.color;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        // Create button text
        GameObject textObj = CreateText("Text", btnObj.transform, buttonText, 20, FontStyles.Bold);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    private GameObject CreateDropdown(string name, Transform parent, string labelText)
    {
        // Create dropdown container
        GameObject dropdownContainer = new GameObject(name);
        dropdownContainer.transform.SetParent(parent, false);

        RectTransform containerRect = dropdownContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0.5f);
        containerRect.anchorMax = new Vector2(1, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        HorizontalLayoutGroup containerLayout = dropdownContainer.AddComponent<HorizontalLayoutGroup>();
        containerLayout.spacing = 10;
        containerLayout.childControlWidth = false;
        containerLayout.childControlHeight = true;
        containerLayout.childForceExpandWidth = false;
        containerLayout.childForceExpandHeight = true;

        // Create label
        GameObject labelObj = CreateText("Label", dropdownContainer.transform, labelText, 22, FontStyles.Bold);
        TextMeshProUGUI labelTMP = labelObj.GetComponent<TextMeshProUGUI>();
        labelTMP.alignment = TextAlignmentOptions.Left;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 120;
        labelLayout.flexibleWidth = 0;

        // Create dropdown
        GameObject dropdownObj = new GameObject("Dropdown");
        dropdownObj.transform.SetParent(dropdownContainer.transform, false);

        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Add color transitions for better UX
        ColorBlock dropdownColors = dropdown.colors;
        dropdownColors.normalColor = dropdownBg.color;
        dropdownColors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        dropdownColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        dropdownColors.selectedColor = dropdownBg.color;
        dropdownColors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        dropdownColors.colorMultiplier = 1f;
        dropdown.colors = dropdownColors;

        RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
        dropdownRect.anchorMin = Vector2.zero;
        dropdownRect.anchorMax = Vector2.one;
        dropdownRect.sizeDelta = Vector2.zero;
        dropdownRect.anchoredPosition = Vector2.zero;
        
        LayoutElement dropdownLayout = dropdownObj.AddComponent<LayoutElement>();
        dropdownLayout.flexibleWidth = 1f;
        dropdownLayout.preferredHeight = 45;
        dropdownLayout.minHeight = 45;

        // Create label for dropdown (the selected text)
        GameObject dropdownLabelObj = CreateText("Label", dropdownObj.transform, "Select...", 20, FontStyles.Normal);
        TextMeshProUGUI dropdownLabelTMP = dropdownLabelObj.GetComponent<TextMeshProUGUI>();
        dropdownLabelTMP.alignment = TextAlignmentOptions.Left;
        
        RectTransform dropdownLabelRect = dropdownLabelObj.GetComponent<RectTransform>();
        dropdownLabelRect.anchorMin = new Vector2(0, 0);
        dropdownLabelRect.anchorMax = new Vector2(1, 1);
        dropdownLabelRect.offsetMin = new Vector2(10, 5);
        dropdownLabelRect.offsetMax = new Vector2(-25, -5);

        dropdown.captionText = dropdownLabelTMP;

        // Create arrow indicator
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-15, 0);
        
        TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▼";
        arrowText.fontSize = 14;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = Color.white;
        
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
            font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
        if (font != null)
            arrowText.font = font;

        // Create template for dropdown list
        // Template should be a CHILD of the dropdown GameObject (as shown in Unity's manual setup)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false); // Child of dropdown
        templateObj.SetActive(false);

        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        // Template will be positioned by Unity's dropdown system
        // Anchor to top-left, pivot at top-center for dropdown expansion
        templateRect.anchorMin = new Vector2(0, 1);
        templateRect.anchorMax = new Vector2(1, 1);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = Vector2.zero;
        templateRect.sizeDelta = new Vector2(0, 200); // Height for dropdown list

        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        // Add shadow/border effect
        Shadow templateShadow = templateObj.AddComponent<Shadow>();
        templateShadow.effectColor = new Color(0, 0, 0, 0.5f);
        templateShadow.effectDistance = new Vector2(2, -2);

        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.clear; // Transparent mask
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        scrollRect.viewport = viewportRect;

        // Create content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.anchoredPosition = Vector2.zero;

        // Note: ToggleGroup is optional - Unity's dropdown handles selection internally
        // We'll add it to the item template instead if needed

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 2; // Add spacing between dropdown items to prevent overlap
        contentLayout.childControlHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.padding = new RectOffset(2, 2, 2, 2); // Add padding to prevent items touching edges

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        // Create item template - MUST be a child of Content (NOT Template directly!)
        // Structure should match Unity's manual setup: Template > Viewport > Content > Item
        GameObject itemTemplate = new GameObject("Item");
        itemTemplate.transform.SetParent(contentObj.transform, false);
        itemTemplate.SetActive(false); // Template items should be inactive

        // Add RectTransform to Item
        RectTransform itemRect = itemTemplate.GetComponent<RectTransform>();
        if (itemRect == null)
            itemRect = itemTemplate.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.sizeDelta = new Vector2(0, 45); // Increased height to prevent text overlap
        itemRect.anchoredPosition = Vector2.zero;
        
        // Add LayoutElement to ensure proper sizing
        LayoutElement itemLayoutElement = itemTemplate.AddComponent<LayoutElement>();
        itemLayoutElement.minHeight = 45;
        itemLayoutElement.preferredHeight = 45;
        itemLayoutElement.flexibleHeight = 0;

        // Add Toggle component to Item - THIS IS CRITICAL
        Toggle itemToggle = itemTemplate.AddComponent<Toggle>();
        itemToggle.isOn = false;
        
        // Create Item Background as a CHILD of Item (like Unity's manual setup)
        GameObject itemBgObj = new GameObject("Item Background");
        itemBgObj.transform.SetParent(itemTemplate.transform, false);
        RectTransform itemBgRect = itemBgObj.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.sizeDelta = Vector2.zero;
        itemBgRect.anchoredPosition = Vector2.zero;
        
        Image itemBg = itemBgObj.AddComponent<Image>();
        itemBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        itemToggle.targetGraphic = itemBg;
        
        // Set up toggle colors
        ColorBlock toggleColors = itemToggle.colors;
        toggleColors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        toggleColors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        toggleColors.pressedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        toggleColors.selectedColor = new Color(0.35f, 0.5f, 0.7f, 1f);
        toggleColors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        toggleColors.colorMultiplier = 1f;
        itemToggle.colors = toggleColors;

        // Create Item Checkmark as a CHILD of Item (like Unity's manual setup)
        GameObject itemCheckmarkObj = new GameObject("Item Checkmark");
        itemCheckmarkObj.transform.SetParent(itemTemplate.transform, false);
        RectTransform itemCheckmarkRect = itemCheckmarkObj.AddComponent<RectTransform>();
        itemCheckmarkRect.anchorMin = new Vector2(0, 0.5f);
        itemCheckmarkRect.anchorMax = new Vector2(0, 0.5f);
        itemCheckmarkRect.pivot = new Vector2(0.5f, 0.5f);
        itemCheckmarkRect.sizeDelta = new Vector2(20, 20);
        itemCheckmarkRect.anchoredPosition = new Vector2(10, 0);
        
        Image itemCheckmark = itemCheckmarkObj.AddComponent<Image>();
        itemCheckmark.color = Color.white;
        // Use a simple checkmark symbol (✓) or you can assign a sprite later
        itemToggle.graphic = itemCheckmark; // This shows when toggle is on

        // Create Item Label as a CHILD of Item (like Unity's manual setup)
        GameObject itemLabelObj = CreateText("Item Label", itemTemplate.transform, "Option", 18, FontStyles.Normal);
        TextMeshProUGUI itemLabelTMP = itemLabelObj.GetComponent<TextMeshProUGUI>();
        itemLabelTMP.alignment = TextAlignmentOptions.Left;
        itemLabelTMP.enableWordWrapping = true;
        itemLabelTMP.overflowMode = TextOverflowModes.Overflow; // Allow text to expand vertically
        itemLabelTMP.autoSizeTextContainer = false;
        
        RectTransform itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = new Vector2(0, 0);
        itemLabelRect.anchorMax = new Vector2(1, 1);
        itemLabelRect.offsetMin = new Vector2(35, 5); // Leave space for checkmark, add padding
        itemLabelRect.offsetMax = new Vector2(-10, -5); // Add padding
        
        // Don't add ContentSizeFitter to label - it's constrained by the item's fixed height
        // The label will wrap text within the item's bounds

        // Ensure item template is the first child of Content (Unity may expect this)
        itemTemplate.transform.SetAsFirstSibling();
        
        // Assign template and item references to dropdown
        dropdown.template = templateRect;
        dropdown.itemText = itemLabelTMP;
        dropdown.itemImage = itemBg; // Item Background image
        
        // Ensure dropdown is properly configured
        dropdown.captionText.text = "Select...";
        dropdown.value = -1; // Start with no selection
        
        // Verify the structure is correct
        if (itemTemplate.GetComponent<Toggle>() == null)
        {
            Debug.LogError("Dropdown item template is missing Toggle component!");
        }
        if (contentObj.transform.childCount == 0 || contentObj.transform.GetChild(0).GetComponent<Toggle>() == null)
        {
            Debug.LogError("Dropdown Content does not have a child with Toggle component! Structure may be incorrect.");
        }
        
        // Ensure there are NO Item GameObjects as direct children of Template (only under Content)
        // This was the bug - we were accidentally creating Items in the wrong place
        
        // Don't add dummy options here - let DropdownPuzzle handle that when loading questions

        return dropdownContainer; // Return container so we can find both label and dropdown
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

