using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// A simple math puzzle that generates random addition/subtraction problems.
/// </summary>
public class SimpleMathPuzzle : MonoBehaviour, IPuzzle
{
    [Serializable]
    private struct CustomQuestion
    {
        [TextArea]
        public string prompt;
        public int correctAnswer;
    }

    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Puzzle Settings")]
    [SerializeField] private int minNumber = 1;
    [SerializeField] private int maxNumber = 20;
    [SerializeField] private bool allowSubtraction = true;
    [SerializeField] private bool allowMultiplication = false;
    
    [Header("Custom Question Set")]
    [Tooltip("If enabled, the puzzle will use the questions below instead of generating random math problems.")]
    [SerializeField] private bool useCustomQuestions = false;
    [Tooltip("Optional list of predefined questions with their answers. Leave empty to keep using generated puzzles.")]
    [SerializeField] private CustomQuestion[] customQuestions = Array.Empty<CustomQuestion>();
    [Tooltip("If true, select a random custom question each time. If false, the list is used in order.")]
    [SerializeField] private bool randomizeCustomQuestions = false;

    private int correctAnswer;
    private Action onCompleteCallback;
    private Action onCancelCallback;
    private bool isCompleted;
    private int sequentialQuestionIndex;

    public bool IsActive => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsCompleted => isCompleted;

    void Awake()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);
        
        if (submitButton)
            submitButton.onClick.AddListener(OnSubmitClicked);
        
        if (cancelButton)
            cancelButton.onClick.AddListener(OnCancelClicked);

        if (answerInput)
        {
            answerInput.onSubmit.AddListener((value) => OnSubmitClicked());
        }
    }

    public void ShowPuzzle(Action onComplete, Action onCancel)
    {
        onCompleteCallback = onComplete;
        onCancelCallback = onCancel;

        GenerateQuestion();
        
        if (puzzlePanel) puzzlePanel.SetActive(true);
        if (answerInput)
        {
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
        }
        if (feedbackText) feedbackText.text = "";

        // Lock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePuzzle()
    {
        if (puzzlePanel) puzzlePanel.SetActive(false);
        
        // Restore cursor state (you may need to adjust based on your game)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ResetPuzzle()
    {
        isCompleted = false;
        correctAnswer = 0;
        sequentialQuestionIndex = 0;
        if (answerInput) answerInput.text = "";
        if (feedbackText) feedbackText.text = "";
        HidePuzzle();
    }

    private void GenerateQuestion()
    {
        if (useCustomQuestions && customQuestions != null && customQuestions.Length > 0)
        {
            var questionData = GetNextCustomQuestion();
            if (!string.IsNullOrWhiteSpace(questionData.prompt))
            {
                correctAnswer = questionData.correctAnswer;
                if (questionText)
                    questionText.text = questionData.prompt;
                Debug.Log($"Loaded custom puzzle question: {questionData.prompt} (Answer: {correctAnswer})");
                return;
            }
            Debug.LogWarning("SimpleMathPuzzle: Custom question had an empty prompt. Falling back to generated question.");
        }

        int num1 = UnityEngine.Random.Range(minNumber, maxNumber + 1);
        int num2 = UnityEngine.Random.Range(minNumber, maxNumber + 1);
        
        // Determine operation
        int operationType = 0; // 0 = addition, 1 = subtraction, 2 = multiplication
        
        if (allowMultiplication && allowSubtraction)
            operationType = UnityEngine.Random.Range(0, 3);
        else if (allowSubtraction)
            operationType = UnityEngine.Random.Range(0, 2);
        else if (allowMultiplication)
            operationType = UnityEngine.Random.Range(0, 2) == 0 ? 0 : 2;

        string question = "";
        
        switch (operationType)
        {
            case 0: // Addition
                correctAnswer = num1 + num2;
                question = $"{num1} + {num2} = ?";
                break;
            
            case 1: // Subtraction
                // Ensure result is positive
                if (num1 < num2)
                {
                    int temp = num1;
                    num1 = num2;
                    num2 = temp;
                }
                correctAnswer = num1 - num2;
                question = $"{num1} - {num2} = ?";
                break;
            
            case 2: // Multiplication
                // Use smaller numbers for multiplication
                num1 = UnityEngine.Random.Range(2, 11);
                num2 = UnityEngine.Random.Range(2, 11);
                correctAnswer = num1 * num2;
                question = $"{num1} × {num2} = ?";
                break;
        }

        if (questionText)
            questionText.text = question;

        Debug.Log($"Generated puzzle: {question} (Answer: {correctAnswer})");
    }

    private CustomQuestion GetNextCustomQuestion()
    {
        if (customQuestions == null || customQuestions.Length == 0)
        {
            return default;
        }

        if (randomizeCustomQuestions)
        {
            int index = UnityEngine.Random.Range(0, customQuestions.Length);
            return customQuestions[index];
        }

        // Clamp index to valid range
        int clampedIndex = Mathf.Clamp(sequentialQuestionIndex, 0, customQuestions.Length - 1);
        var question = customQuestions[clampedIndex];
        
        // Only advance index if we're not at the end (to allow cycling)
        if (sequentialQuestionIndex < customQuestions.Length - 1)
        {
            sequentialQuestionIndex++;
        }
        else
        {
            // Wrap around to beginning
            sequentialQuestionIndex = 0;
        }
        
        return question;
    }

    private void OnSubmitClicked()
    {
        if (string.IsNullOrEmpty(answerInput?.text))
        {
            ShowFeedback("Please enter an answer!", Color.yellow);
            return;
        }

        if (int.TryParse(answerInput.text, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                ShowFeedback("Correct! Unlocking...", Color.green);
                isCompleted = true;
                Invoke(nameof(CompleteWithDelay), 0.5f);
            }
            else
            {
                ShowFeedback("Incorrect! Try again.", Color.red);
                answerInput.text = "";
                answerInput.Select();
                answerInput.ActivateInputField();
            }
        }
        else
        {
            ShowFeedback("Please enter a valid number!", Color.yellow);
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
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

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }
}

