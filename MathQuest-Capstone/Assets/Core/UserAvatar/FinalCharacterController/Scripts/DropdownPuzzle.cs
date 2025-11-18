using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Puzzle that uses dropdown menus for answers (e.g., Name + Function questions).
/// Separate from MultipleChoicePuzzle which uses buttons.
/// </summary>
public class DropdownPuzzle : MonoBehaviour, IPuzzle
{
    [Serializable]
    public struct ChoiceOption
    {
        [TextArea]
        public string label;
    }

    [Serializable]
    public struct DropdownGroup
    {
        [Tooltip("Label/header for this dropdown (e.g., 'Nom:', 'Fonction:')")]
        public string label;
        [Tooltip("Options available in this dropdown")]
        public ChoiceOption[] options;
        [Tooltip("Index of the correct option (0-based)")]
        public int correctOptionIndex;
    }

    [Serializable]
    public struct DropdownQuestion
    {
        [TextArea]
        public string prompt;
        [Tooltip("Optional helper image (leave empty for text-only questions).")]
        public Sprite promptImage;
        [Tooltip("Dropdown groups (e.g., Name + Function). At least one required.")]
        public DropdownGroup[] dropdownGroups;
    }

    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptImage;
    [SerializeField] private TMP_Dropdown[] dropdownMenus;
    [SerializeField] private TextMeshProUGUI[] dropdownLabels;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Puzzle Settings")]
    [SerializeField] private DropdownQuestion[] questions = Array.Empty<DropdownQuestion>();
    [Tooltip("Randomly pick a question each time instead of using them in order.")]
    [SerializeField] private bool randomizeQuestionOrder = false;
    [Tooltip("Shuffle option order each time a question is shown.")]
    [SerializeField] private bool randomizeOptions = false;
    [Tooltip("Seconds to wait after a correct answer before completing the puzzle.")]
    [SerializeField] private float completionDelay = 0.4f;

    private Action onCompleteCallback;
    private Action onCancelCallback;
    private bool isCompleted;
    private int sequentialQuestionIndex;
    private DropdownQuestion currentQuestion;

    public bool IsActive => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void ShowPuzzle(Action onComplete, Action onCancel)
    {
        onCompleteCallback = onComplete;
        onCancelCallback = onCancel;
        isCompleted = false;

        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("DropdownPuzzle: No questions configured.");
            onCancelCallback?.Invoke();
            return;
        }

        // Get question at current index without advancing (advance happens on completion)
        DropdownQuestion question = GetQuestionAtCurrentIndex();
        LoadQuestion(question);

        if (puzzlePanel) puzzlePanel.SetActive(true);
        if (feedbackText) feedbackText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePuzzle()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ResetPuzzle()
    {
        isCompleted = false;
        // Don't reset sequentialQuestionIndex - keep it global so questions advance across all interactables
        if (feedbackText) feedbackText.text = "";
        HidePuzzle();
    }

    private DropdownQuestion GetQuestionAtCurrentIndex()
    {
        if (questions == null || questions.Length == 0)
            return default;

        if (randomizeQuestionOrder)
        {
            int randomIndex = UnityEngine.Random.Range(0, questions.Length);
            return questions[randomIndex];
        }

        // Clamp index to valid range and return question at that index
        int clampedIndex = Mathf.Clamp(sequentialQuestionIndex, 0, questions.Length - 1);
        return questions[clampedIndex];
    }

    /// <summary>
    /// Advance to the next question index. Called after a question is completed.
    /// </summary>
    public void AdvanceQuestionIndex()
    {
        if (questions == null || questions.Length == 0)
            return;

        if (randomizeQuestionOrder)
            return; // Don't advance for random questions

        // Advance index, wrapping around if needed
        if (sequentialQuestionIndex < questions.Length - 1)
        {
            sequentialQuestionIndex++;
        }
        else
        {
            // Wrap around to beginning
            sequentialQuestionIndex = 0;
        }
    }

    private void LoadQuestion(DropdownQuestion question)
    {
        currentQuestion = question;
        
        if (promptText)
            promptText.text = question.prompt ?? string.Empty;

        if (promptImage != null)
        {
            bool hasImage = question.promptImage != null;
            promptImage.gameObject.SetActive(hasImage);
            promptImage.sprite = question.promptImage;
            
            // Update LayoutElement to ignore layout when no image
            LayoutElement imageLayout = promptImage.GetComponent<LayoutElement>();
            if (imageLayout != null)
            {
                imageLayout.ignoreLayout = !hasImage;
            }
        }

        var dropdownGroups = question.dropdownGroups;
        if (dropdownGroups == null || dropdownGroups.Length == 0)
        {
            Debug.LogWarning("DropdownPuzzle: Question has no dropdownGroups!");
            return;
        }

        for (int i = 0; i < dropdownMenus.Length; i++)
        {
            bool hasDropdown = i < dropdownGroups.Length;

            if (dropdownMenus[i] != null)
                dropdownMenus[i].gameObject.SetActive(hasDropdown);

            if (dropdownLabels.Length > i && dropdownLabels[i] != null)
                dropdownLabels[i].gameObject.SetActive(hasDropdown);

            if (!hasDropdown)
                continue;

            var group = dropdownGroups[i];
            var dropdown = dropdownMenus[i];
            
            if (dropdown == null)
                continue;

            // Set label
            if (dropdownLabels.Length > i && dropdownLabels[i] != null)
                dropdownLabels[i].text = group.label ?? string.Empty;

            // Prepare options
            var optionData = PrepareOptionData(group.options);
            dropdown.ClearOptions();

            List<string> optionTexts = new List<string>();
            foreach (var option in optionData)
            {
                optionTexts.Add(option.label ?? string.Empty);
            }

            dropdown.AddOptions(optionTexts);
            
            // Reset dropdown to first option and refresh
            // Use -1 first to clear, then set to 0 to show first option
            dropdown.value = -1;
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            
            // Ensure dropdown is properly configured
            if (dropdown.template == null)
            {
                Debug.LogWarning($"DropdownPuzzle: Dropdown {i} has no template assigned! Dropdown will not work properly.");
            }
            
            // Ensure template is hidden (should be by default, but double-check)
            if (dropdown.template != null && dropdown.template.gameObject.activeSelf)
            {
                dropdown.template.gameObject.SetActive(false);
            }
        }

        // Show submit button
        if (submitButton != null)
            submitButton.gameObject.SetActive(true);
    }

    private List<ChoiceOption> PrepareOptionData(ChoiceOption[] source)
    {
        List<ChoiceOption> result = new List<ChoiceOption>();

        if (source != null)
            result.AddRange(source);

        if (randomizeOptions)
        {
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }
        }

        return result;
    }

    private void OnSubmitClicked()
    {
        if (currentQuestion.dropdownGroups == null)
            return;

        // Check if all dropdowns have the correct selection
        bool allCorrect = true;
        for (int i = 0; i < dropdownMenus.Length && i < currentQuestion.dropdownGroups.Length; i++)
        {
            if (dropdownMenus[i] == null || !dropdownMenus[i].gameObject.activeSelf)
                continue;

            var group = currentQuestion.dropdownGroups[i];
            int selectedIndex = dropdownMenus[i].value;

            if (selectedIndex != group.correctOptionIndex)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            ShowFeedback("Incorrect! Please check your selections and try again.", Color.red);
        }
    }

    private void HandleCorrectAnswer()
    {
        isCompleted = true;
        ShowFeedback("Correct! Unlocking...", Color.green);
        SetDropdownInteractivity(false);
        
        // Advance to next question for next time
        AdvanceQuestionIndex();
        
        Invoke(nameof(CompleteWithDelay), completionDelay);
    }

    private void CompleteWithDelay()
    {
        HidePuzzle();
        onCompleteCallback?.Invoke();
    }

    private void OnCancelClicked()
    {
        HidePuzzle();
        onCancelCallback?.Invoke();
    }

    private void SetDropdownInteractivity(bool enabled)
    {
        foreach (var dropdown in dropdownMenus)
        {
            if (dropdown != null)
                dropdown.interactable = enabled;
        }

        if (submitButton != null)
            submitButton.interactable = enabled;
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = color;
    }
}

