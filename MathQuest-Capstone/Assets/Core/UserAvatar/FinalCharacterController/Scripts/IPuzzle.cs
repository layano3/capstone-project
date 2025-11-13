using UnityEngine;
using System;

/// <summary>
/// Interface for all puzzle types that can be used to unlock interactables.
/// </summary>
public interface IPuzzle
{
    /// <summary>
    /// Called when the puzzle should be shown to the player.
    /// </summary>
    /// <param name="onComplete">Callback to invoke when puzzle is completed successfully.</param>
    /// <param name="onCancel">Callback to invoke when puzzle is cancelled.</param>
    void ShowPuzzle(Action onComplete, Action onCancel);

    /// <summary>
    /// Called to hide/cleanup the puzzle UI.
    /// </summary>
    void HidePuzzle();

    /// <summary>
    /// Check if the puzzle is currently active/visible.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Check if the puzzle has been completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Reset the puzzle to its initial state.
    /// </summary>
    void ResetPuzzle();
}

