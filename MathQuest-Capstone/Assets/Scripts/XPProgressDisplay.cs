using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable presenter that displays the player's level, XP progress, and level-up events.
/// Works with either a Slider (normalized 0-1) or a filled Image, plus optional text labels.
/// </summary>
[DisallowMultipleComponent]
public class XPProgressDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text xpLabel;

    [Header("Formatting")]
    [SerializeField] private string levelFormat = "Level {0}";
    [SerializeField] private string xpFormat = "{0}/{1} XP";
    [SerializeField] private string maxLevelXpFormat = "{0} XP (MAX)";
    [SerializeField] private bool autoConfigureSliderRange = true;

    private int currentXP;
    private int currentLevel = LevelCalculator.GetMinLevel();

    /// <summary>
    /// Event fired whenever the calculated level changes.
    /// </summary>
    public event Action<int> OnLevelChanged;

    /// <summary>
    /// Sets the current XP and refreshes the view.
    /// </summary>
    public void SetXP(int totalXP)
    {
        totalXP = Mathf.Max(0, totalXP);
        int newLevel = LevelCalculator.CalculateLevel(totalXP);
        int xpProgress = LevelCalculator.CalculateXPProgress(totalXP, newLevel);
        int xpNeeded = LevelCalculator.CalculateXPForNextLevel(newLevel);

        UpdateUI(newLevel, xpProgress, xpNeeded);

        if (newLevel != currentLevel)
        {
            OnLevelChanged?.Invoke(newLevel);
        }

        currentLevel = newLevel;
        currentXP = totalXP;
    }

    /// <summary>
    /// Adds XP (positive or negative) and updates the view.
    /// </summary>
    public void AddXP(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        SetXP(currentXP + delta);
    }

    private void UpdateUI(int level, int xpProgress, int xpNeeded)
    {
        float normalized = xpNeeded <= 0 || xpNeeded == int.MaxValue
            ? 1f
            : Mathf.Clamp01((float)xpProgress / xpNeeded);

        if (progressSlider != null)
        {
            if (autoConfigureSliderRange)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.wholeNumbers = false;
            }

            progressSlider.value = normalized;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = normalized;
        }

        if (levelLabel != null)
        {
            levelLabel.text = string.Format(levelFormat, level);
        }

        if (xpLabel != null)
        {
            if (xpNeeded == int.MaxValue)
            {
                xpLabel.text = string.Format(maxLevelXpFormat, xpProgress);
            }
            else
            {
                xpLabel.text = string.Format(xpFormat, xpProgress, xpNeeded);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (progressSlider != null && autoConfigureSliderRange)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.wholeNumbers = false;
        }
    }
#endif
}

