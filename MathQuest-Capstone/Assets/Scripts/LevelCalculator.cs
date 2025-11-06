using UnityEngine;

public static class LevelCalculator
{
    private const int BASE_XP = 100;
    private const float EXPONENTIAL_MULTIPLIER = 1.5f;
    private const int MIN_LEVEL = 1;
    private const int MAX_LEVEL = 100;

    public static int CalculateLevel(int currentXP)
    {
        if (currentXP < 0)
        {
            return MIN_LEVEL;
        }

        int level = MIN_LEVEL;
        int xpRequired = BASE_XP;
        int totalXP = 0;

        while (totalXP + xpRequired <= currentXP && level < MAX_LEVEL)
        {
            totalXP += xpRequired;
            level++;
            xpRequired = CalculateXPForNextLevel(level);
        }

        return level;
    }

    public static int CalculateXPForNextLevel(int currentLevel)
    {
        if (currentLevel < MIN_LEVEL)
        {
            return BASE_XP;
        }

        if (currentLevel >= MAX_LEVEL)
        {
            return int.MaxValue;
        }

        return Mathf.RoundToInt(BASE_XP * Mathf.Pow(EXPONENTIAL_MULTIPLIER, currentLevel - 1));
    }

    public static int CalculateTotalXPForLevel(int level)
    {
        if (level <= MIN_LEVEL)
        {
            return 0;
        }

        if (level > MAX_LEVEL)
        {
            level = MAX_LEVEL;
        }

        int totalXP = 0;
        for (int i = MIN_LEVEL; i < level; i++)
        {
            totalXP += CalculateXPForNextLevel(i);
        }

        return totalXP;
    }

    public static int CalculateXPProgress(int currentXP, int currentLevel)
    {
        int xpForCurrentLevel = CalculateTotalXPForLevel(currentLevel);
        return currentXP - xpForCurrentLevel;
    }

    public static float CalculateProgressPercentage(int currentXP, int currentLevel)
    {
        int xpProgress = CalculateXPProgress(currentXP, currentLevel);
        int xpNeeded = CalculateXPForNextLevel(currentLevel);

        if (xpNeeded == 0 || xpNeeded == int.MaxValue)
        {
            return 1.0f;
        }

        return Mathf.Clamp01((float)xpProgress / xpNeeded);
    }

    public static bool IsMaxLevel(int level)
    {
        return level >= MAX_LEVEL;
    }

    public static int GetMaxLevel()
    {
        return MAX_LEVEL;
    }

    public static int GetMinLevel()
    {
        return MIN_LEVEL;
    }

    public static int CalculateXPToNextLevel(int currentXP, int currentLevel)
    {
        int xpProgress = CalculateXPProgress(currentXP, currentLevel);
        int xpNeeded = CalculateXPForNextLevel(currentLevel);
        return Mathf.Max(0, xpNeeded - xpProgress);
    }
}

