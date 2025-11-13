using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// A simple math puzzle that generates random addition/subtraction problems.
/// </summary>
public class SimpleMathPuzzle : MonoBehaviour, IPuzzle
{
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

    private int correctAnswer;
    private Action onCompleteCallback;
    private Action onCancelCallback;
    private bool isCompleted;

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
        if (answerInput) answerInput.text = "";
        if (feedbackText) feedbackText.text = "";
        HidePuzzle();
    }

    private void GenerateQuestion()
    {
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

