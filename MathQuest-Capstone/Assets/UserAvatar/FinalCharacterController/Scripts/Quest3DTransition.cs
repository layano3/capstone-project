using UnityEngine;
using DG.Tweening;
using System;

public class Quest3DTransition : MonoBehaviour
{
    [Header("Common Settings")]
    public float transitionDuration = 1f;
    // Final on-screen position for a panel.
    public Vector3 endPos = new Vector3(0f, -37f, 0f);

    [Header("Next Transition Settings")]
    // For Next: outgoing panel should move from (0, -37, 0) to (-420, -37, 200)
    public Vector3 nextCurrentEnd = new Vector3(-420f, -37f, 200f);
    // For Next: incoming panel starts at (400, -37, 200)
    public Vector3 nextIncomingStart = new Vector3(400f, -37f, 200f);

    [Header("Previous Transition Settings")]
    // For Previous: outgoing panel should move from (0, -37, 0) to (400, -37, 200)
    public Vector3 previousCurrentEnd = new Vector3(400f, -37f, 200f);
    // For Previous: incoming panel starts at (-420, -37, 200)
    public Vector3 previousIncomingStart = new Vector3(-420f, -37f, 200f);

    /// <summary>
    /// Animate transition between two panels.
    /// </summary>
    /// <param name="current">Currently visible panel.</param>
    /// <param name="incoming">Panel that will animate in.</param>
    /// <param name="isNext">True for Next transition; false for Previous.</param>
    /// <param name="onComplete">Callback when animation finishes.</param>
    public void TransitionBetweenPanels(RectTransform current, RectTransform incoming, bool isNext, Action onComplete = null)
    {
        // Set current panel to the on-screen position (if needed).
        current.localPosition = endPos;

        if (isNext)
        {
            // Set incoming panel's starting position for Next.
            incoming.localPosition = nextIncomingStart;
            // Animate current panel to its target position.
            current.DOLocalMove(nextCurrentEnd, transitionDuration);
            // Animate incoming panel from nextIncomingStart to on-screen position.
            incoming.DOLocalMove(endPos, transitionDuration).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        else // previous
        {
            // Set incoming panel's starting position for Previous.
            incoming.localPosition = previousIncomingStart;
            // Animate current panel to its target position.
            current.DOLocalMove(previousCurrentEnd, transitionDuration);
            // Animate incoming panel from previousIncomingStart to on-screen position.
            incoming.DOLocalMove(endPos, transitionDuration).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
    }
}
