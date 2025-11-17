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

    private Action onCompleteCallback;
    private Action onCancelCallback;
    private bool isCompleted;
    private int sequentialQuestionIndex;
    private readonly List<ChoiceOption> activeOptions = new List<ChoiceOption>();

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
    }

    public void ShowPuzzle(Action onComplete, Action onCancel)
    {
        onCompleteCallback = onComplete;
        onCancelCallback = onCancel;
        isCompleted = false;

        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("MultipleChoicePuzzle: No questions configured.");
            onCancelCallback?.Invoke();
            return;
        }

        LoadQuestion(GetNextQuestion());

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
        sequentialQuestionIndex = 0;
        if (feedbackText) feedbackText.text = "";
        HidePuzzle();
    }

    private MultipleChoiceQuestion GetNextQuestion()
    {
        if (questions == null || questions.Length == 0)
            return default;

        if (randomizeQuestionOrder)
        {
            int randomIndex = UnityEngine.Random.Range(0, questions.Length);
            return questions[randomIndex];
        }

        var question = questions[Mathf.Clamp(sequentialQuestionIndex, 0, questions.Length - 1)];
        sequentialQuestionIndex = (sequentialQuestionIndex + 1) % questions.Length;
        return question;
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
        }

        activeOptions.Clear();
        var optionData = PrepareOptionData(question.options);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasOption = i < optionData.Count;

            if (optionButtons[i] != null)
                optionButtons[i].gameObject.SetActive(hasOption);

            if (optionLabels.Length > i && optionLabels[i] != null)
                optionLabels[i].gameObject.SetActive(hasOption);

            if (!hasOption)
                continue;

            var data = optionData[i];
            activeOptions.Add(data);

            if (optionButtons[i] != null)
                optionButtons[i].interactable = true;

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
            return;

        var selectedOption = activeOptions[optionIndex];
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
        ShowFeedback("Correct! Unlocking...", Color.green);
        SetOptionInteractivity(false);
        Invoke(nameof(CompleteWithDelay), completionDelay);
    }

    private void HandleIncorrectAnswer(int selectedIndex)
    {
        ShowFeedback("Incorrect! Try again.", Color.red);
        if (selectedIndex >= 0 && selectedIndex < optionButtons.Length && optionButtons[selectedIndex] != null)
        {
            optionButtons[selectedIndex].interactable = false;
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

