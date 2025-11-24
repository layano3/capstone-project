using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Generic multiple-choice puzzle that can optionally display a supporting image.
/// Keeps the same queueing behavior as SimpleMathPuzzle by serving questions sequentially or at random.
/// </summary>
public class MultipleChoicePuzzle : MonoBehaviour, IPuzzle
{
    [Serializable]
    public struct ChoiceOption
    {
        [TextArea]
        public string label;
        [Tooltip("Mark true for the correct option (supports multiple correct answers if needed).")]
        public bool isCorrect;
    }

    [Serializable]
    public struct MultipleChoiceQuestion
    {
        [TextArea]
        public string prompt;
        [Tooltip("Optional helper image (leave empty for text-only questions).")]
        public Sprite promptImage;
        [Tooltip("Options for button-based questions")]
        public ChoiceOption[] options;
    }

    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptImage;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Puzzle Settings")]
    [SerializeField] private MultipleChoiceQuestion[] questions = Array.Empty<MultipleChoiceQuestion>();
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
    private readonly List<ChoiceOption> activeOptions = new List<ChoiceOption>();
    private int wrongAttemptCount = 0; // Track wrong attempts for current question
    private bool penaltyApplied = false; // Track if penalty has been applied for this puzzle session
    private PlayerXPTracker xpTracker; // Cache the XP tracker for better performance

    public bool IsActive => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int capturedIndex = i;
            optionButtons[i]?.onClick.AddListener(() => OnOptionSelected(capturedIndex));
        }

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Cache the XP tracker for better performance
        xpTracker = FindObjectOfType<PlayerXPTracker>();
        if (xpTracker == null && enableXPPenalty)
        {
            Debug.LogWarning("MultipleChoicePuzzle: PlayerXPTracker not found in scene. XP penalties will not work!");
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
        
        Debug.Log($"MultipleChoicePuzzle: Showing puzzle. XP penalty enabled: {enableXPPenalty}, threshold: {wrongAttemptsBeforePenalty}, amount: {xpPenaltyAmount}");

        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("MultipleChoicePuzzle: No questions configured.");
            onCancelCallback?.Invoke();
            return;
        }

        // Get question at current index without advancing (advance happens on completion)
        MultipleChoiceQuestion question = GetQuestionAtCurrentIndex();
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

    private MultipleChoiceQuestion GetQuestionAtCurrentIndex()
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

    private void LoadQuestion(MultipleChoiceQuestion question)
    {
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

        activeOptions.Clear();
        var optionData = PrepareOptionData(question.options);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasOption = i < optionData.Count;

            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(hasOption);
                // Always re-enable buttons when loading a new question
                if (hasOption)
                {
                    optionButtons[i].interactable = true;
                }
            }

            if (optionLabels.Length > i && optionLabels[i] != null)
                optionLabels[i].gameObject.SetActive(hasOption);

            if (!hasOption)
                continue;

            var data = optionData[i];
            activeOptions.Add(data);

            if (optionLabels.Length > i && optionLabels[i] != null)
                optionLabels[i].text = data.label ?? string.Empty;
        }
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

    private void OnOptionSelected(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= activeOptions.Count)
        {
            Debug.LogWarning($"MultipleChoicePuzzle: Invalid option index {optionIndex} (activeOptions count: {activeOptions.Count})");
            return;
        }

        var selectedOption = activeOptions[optionIndex];
        Debug.Log($"MultipleChoicePuzzle: Option {optionIndex} selected. Is correct: {selectedOption.isCorrect}");
        
        if (selectedOption.isCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleIncorrectAnswer(optionIndex);
        }
    }

    private void HandleCorrectAnswer()
    {
        isCompleted = true;
        wrongAttemptCount = 0; // Reset wrong attempts on correct answer
        penaltyApplied = false; // Reset penalty flag on correct answer
        ShowFeedback("Correct! Unlocking...", Color.green);
        SetOptionInteractivity(false);
        
        // Advance to next question for next time
        AdvanceQuestionIndex();
        
        Invoke(nameof(CompleteWithDelay), completionDelay);
    }

    private void HandleIncorrectAnswer(int selectedIndex)
    {
        wrongAttemptCount++;
        Debug.Log($"MultipleChoicePuzzle: Wrong attempt #{wrongAttemptCount} (threshold: {wrongAttemptsBeforePenalty}, penalty enabled: {enableXPPenalty})");
        
        // Check if XP penalty should be applied
        if (enableXPPenalty && wrongAttemptCount >= wrongAttemptsBeforePenalty)
        {
            Debug.Log($"MultipleChoicePuzzle: Threshold reached! Attempting to apply penalty. Penalty already applied: {penaltyApplied}");
            DeductXPForWrongAttempts();
        }
        else if (!enableXPPenalty)
        {
            Debug.LogWarning($"MultipleChoicePuzzle: XP penalty is disabled! Enable it in the Inspector.");
        }
        
        // Show feedback with attempt count
        string feedbackMessage = wrongAttemptCount >= wrongAttemptsBeforePenalty
            ? $"Incorrect! ({wrongAttemptCount} attempts) - XP penalty applied. Try again."
            : $"Incorrect! Try again. ({wrongAttemptCount}/{wrongAttemptsBeforePenalty} attempts)";
        
        Color feedbackColor = wrongAttemptCount >= wrongAttemptsBeforePenalty 
            ? Color.red 
            : new Color(1f, 0.5f, 0f); // Orange for warning, red for penalty
        
        ShowFeedback(feedbackMessage, feedbackColor);
        
        // Disable the wrong option that was selected (user can still try other wrong options)
        // This allows multiple wrong attempts by clicking different wrong buttons
        if (selectedIndex >= 0 && selectedIndex < optionButtons.Length && optionButtons[selectedIndex] != null)
        {
            optionButtons[selectedIndex].interactable = false;
            Debug.Log($"MultipleChoicePuzzle: Disabled wrong option button at index {selectedIndex}. Remaining attempts: {wrongAttemptsBeforePenalty - wrongAttemptCount}");
        }
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
                Debug.Log($"MultipleChoicePuzzle: Deducting {xpPenaltyAmount} XP - {penaltyReason} (Attempt #{wrongAttemptCount})");
                xpTracker.GrantXP(-xpPenaltyAmount, penaltyReason);
            }
            else
            {
                Debug.LogError($"MultipleChoicePuzzle: Cannot deduct XP - PlayerXPTracker not found in scene! Penalty will not be recorded in database.");
            }
        }
        else if (penaltyApplied)
        {
            Debug.Log($"MultipleChoicePuzzle: Penalty already applied for this puzzle session. Skipping duplicate deduction.");
        }
        else
        {
            Debug.LogWarning($"MultipleChoicePuzzle: DeductXPForWrongAttempts called but conditions not met: wrongAttemptCount={wrongAttemptCount}, threshold={wrongAttemptsBeforePenalty}, penaltyApplied={penaltyApplied}");
        }
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

    private void SetOptionInteractivity(bool enabled)
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
                button.interactable = enabled;
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = color;
    }
}

