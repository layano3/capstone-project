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

    [Header("XP Penalty Settings")]
    [Tooltip("Enable XP penalty after multiple wrong attempts")]
    [SerializeField] private bool enableXPPenalty = true;
    [Tooltip("Number of wrong attempts before XP penalty")]
    [SerializeField] private int wrongAttemptsBeforePenalty = 3;
    [Tooltip("Amount of XP to deduct after penalty threshold")]
    [SerializeField] private int xpPenaltyAmount = 10;

    private Action onCompleteCallback;
    private Action onCancelCallback;
    private bool isCompleted;
    private int sequentialQuestionIndex;
    private DropdownQuestion currentQuestion;
    private int wrongAttemptCount = 0; // Track wrong attempts for current question
    private bool penaltyApplied = false; // Track if penalty has been applied for this puzzle session
    private PlayerXPTracker xpTracker; // Cache the XP tracker for better performance

    public bool IsActive => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Cache the XP tracker for better performance
        xpTracker = FindObjectOfType<PlayerXPTracker>();
        if (xpTracker == null && enableXPPenalty)
        {
            Debug.LogWarning("DropdownPuzzle: PlayerXPTracker not found in scene. XP penalties will not work!");
        }
    }

    public void ShowPuzzle(Action onComplete, Action onCancel)
    {
        onCompleteCallback = onComplete;
        onCancelCallback = onCancel;
        isCompleted = false;
        
        // Reset wrong attempt count and penalty flag when showing a new puzzle
        wrongAttemptCount = 0;
        penaltyApplied = false;
        
        Debug.Log($"DropdownPuzzle: Showing puzzle. XP penalty enabled: {enableXPPenalty}, threshold: {wrongAttemptsBeforePenalty}, amount: {xpPenaltyAmount}");

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
        wrongAttemptCount = 0; // Reset wrong attempts when puzzle is reset
        penaltyApplied = false; // Reset penalty flag when puzzle is reset
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
            HandleIncorrectAnswer();
        }
    }
    
    private void HandleIncorrectAnswer()
    {
        wrongAttemptCount++;
        Debug.Log($"DropdownPuzzle: Wrong attempt #{wrongAttemptCount} (threshold: {wrongAttemptsBeforePenalty}, penalty enabled: {enableXPPenalty})");
        
        // Check if XP penalty should be applied
        if (enableXPPenalty && wrongAttemptCount >= wrongAttemptsBeforePenalty)
        {
            Debug.Log($"DropdownPuzzle: Threshold reached! Attempting to apply penalty. Penalty already applied: {penaltyApplied}");
            DeductXPForWrongAttempts();
        }
        else if (!enableXPPenalty)
        {
            Debug.LogWarning($"DropdownPuzzle: XP penalty is disabled! Enable it in the Inspector.");
        }
        
        // Show feedback with attempt count
        string feedbackMessage = wrongAttemptCount >= wrongAttemptsBeforePenalty
            ? $"Incorrect! ({wrongAttemptCount} attempts) - XP penalty applied. Please check your selections and try again."
            : $"Incorrect! Please check your selections and try again. ({wrongAttemptCount}/{wrongAttemptsBeforePenalty} attempts)";
        
        Color feedbackColor = wrongAttemptCount >= wrongAttemptsBeforePenalty 
            ? Color.red 
            : new Color(1f, 0.5f, 0f); // Orange for warning, red for penalty
        
        ShowFeedback(feedbackMessage, feedbackColor);
    }
    
    private void DeductXPForWrongAttempts()
    {
        // Only deduct once when threshold is reached and penalty hasn't been applied yet
        if (wrongAttemptCount >= wrongAttemptsBeforePenalty && !penaltyApplied)
        {
            penaltyApplied = true; // Mark penalty as applied to prevent duplicate deductions
            
            // Use cached tracker, or try to find it if not cached
            if (xpTracker == null)
            {
                xpTracker = FindObjectOfType<PlayerXPTracker>();
            }
            
            if (xpTracker != null)
            {
                string penaltyReason = $"Penalty: {wrongAttemptsBeforePenalty} wrong attempts on puzzle: {gameObject.name}";
                Debug.Log($"DropdownPuzzle: Deducting {xpPenaltyAmount} XP - {penaltyReason} (Attempt #{wrongAttemptCount})");
                xpTracker.GrantXP(-xpPenaltyAmount, penaltyReason);
            }
            else
            {
                Debug.LogError($"DropdownPuzzle: Cannot deduct XP - PlayerXPTracker not found in scene! Penalty will not be recorded in database.");
            }
        }
        else if (penaltyApplied)
        {
            Debug.Log($"DropdownPuzzle: Penalty already applied for this puzzle session. Skipping duplicate deduction.");
        }
        else
        {
            Debug.LogWarning($"DropdownPuzzle: DeductXPForWrongAttempts called but conditions not met: wrongAttemptCount={wrongAttemptCount}, threshold={wrongAttemptsBeforePenalty}, penaltyApplied={penaltyApplied}");
        }
    }

    private void HandleCorrectAnswer()
    {
        isCompleted = true;
        wrongAttemptCount = 0; // Reset wrong attempts on correct answer
        penaltyApplied = false; // Reset penalty flag on correct answer
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

