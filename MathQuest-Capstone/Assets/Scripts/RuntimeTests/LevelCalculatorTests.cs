#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class LevelCalculatorTests
    {
        [Test]
        public void Test_CalculateLevel_ZeroXP_ReturnsLevel1()
        {
            int level = LevelCalculator.CalculateLevel(0);
            Assert.AreEqual(1, level, "Zero XP should return level 1");
        }

        [Test]
        public void Test_CalculateLevel_NegativeXP_ReturnsLevel1()
        {
            int level = LevelCalculator.CalculateLevel(-100);
            Assert.AreEqual(1, level, "Negative XP should return level 1");
        }

        [Test]
        public void Test_CalculateLevel_100XP_ReturnsLevel2()
        {
            int level = LevelCalculator.CalculateLevel(100);
            Assert.AreEqual(2, level, "100 XP should return level 2");
        }

        [Test]
        public void Test_CalculateLevel_250XP_ReturnsLevel3()
        {
            int level = LevelCalculator.CalculateLevel(250);
            Assert.AreEqual(3, level, "250 XP should return level 3");
        }

        [Test]
        public void Test_CalculateLevel_VeryHighXP_DoesNotExceedMaxLevel()
        {
            int level = LevelCalculator.CalculateLevel(int.MaxValue);
            Assert.LessOrEqual(level, LevelCalculator.GetMaxLevel(), "Level should not exceed max level");
        }

        [Test]
        public void Test_CalculateXPForNextLevel_Level1_Returns100()
        {
            int xpNeeded = LevelCalculator.CalculateXPForNextLevel(1);
            Assert.AreEqual(100, xpNeeded, "XP needed for level 2 should be 100");
        }

        [Test]
        public void Test_CalculateXPForNextLevel_Level2_Returns150()
        {
            int xpNeeded = LevelCalculator.CalculateXPForNextLevel(2);
            Assert.AreEqual(150, xpNeeded, "XP needed for level 3 should be 150");
        }

        [Test]
        public void Test_CalculateXPForNextLevel_Level3_Returns225()
        {
            int xpNeeded = LevelCalculator.CalculateXPForNextLevel(3);
            Assert.AreEqual(225, xpNeeded, "XP needed for level 4 should be 225");
        }

        [Test]
        public void Test_CalculateXPForNextLevel_IncreasingProgression()
        {
            int xp1 = LevelCalculator.CalculateXPForNextLevel(1);
            int xp2 = LevelCalculator.CalculateXPForNextLevel(2);
            int xp3 = LevelCalculator.CalculateXPForNextLevel(3);

            Assert.Greater(xp2, xp1, "XP requirement should increase with level");
            Assert.Greater(xp3, xp2, "XP requirement should continue increasing");
        }

        [Test]
        public void Test_CalculateXPForNextLevel_MaxLevel_ReturnsMaxValue()
        {
            int xpNeeded = LevelCalculator.CalculateXPForNextLevel(LevelCalculator.GetMaxLevel());
            Assert.AreEqual(int.MaxValue, xpNeeded, "Max level should require max XP");
        }

        [Test]
        public void Test_CalculateTotalXPForLevel_Level1_Returns0()
        {
            int totalXP = LevelCalculator.CalculateTotalXPForLevel(1);
            Assert.AreEqual(0, totalXP, "Total XP for level 1 should be 0");
        }

        [Test]
        public void Test_CalculateTotalXPForLevel_Level2_Returns100()
        {
            int totalXP = LevelCalculator.CalculateTotalXPForLevel(2);
            Assert.AreEqual(100, totalXP, "Total XP for level 2 should be 100");
        }

        [Test]
        public void Test_CalculateTotalXPForLevel_Level3_Returns250()
        {
            int totalXP = LevelCalculator.CalculateTotalXPForLevel(3);
            Assert.AreEqual(250, totalXP, "Total XP for level 3 should be 250");
        }

        [Test]
        public void Test_CalculateXPProgress_AtLevelStart_Returns0()
        {
            int progress = LevelCalculator.CalculateXPProgress(100, 2);
            Assert.AreEqual(0, progress, "XP progress at level start should be 0");
        }

        [Test]
        public void Test_CalculateXPProgress_MidLevel_ReturnsCorrectProgress()
        {
            int progress = LevelCalculator.CalculateXPProgress(150, 2);
            Assert.AreEqual(50, progress, "XP progress should be 50 for 150 total XP at level 2");
        }

        [Test]
        public void Test_CalculateProgressPercentage_AtLevelStart_Returns0()
        {
            float percentage = LevelCalculator.CalculateProgressPercentage(100, 2);
            Assert.AreEqual(0f, percentage, 0.01f, "Progress percentage at level start should be 0");
        }

        [Test]
        public void Test_CalculateProgressPercentage_HalfwayThrough_Returns50Percent()
        {
            float percentage = LevelCalculator.CalculateProgressPercentage(175, 2);
            Assert.AreEqual(0.5f, percentage, 0.01f, "Progress percentage halfway should be 0.5");
        }

        [Test]
        public void Test_CalculateProgressPercentage_AlmostNextLevel_ReturnsNear100Percent()
        {
            float percentage = LevelCalculator.CalculateProgressPercentage(249, 2);
            Assert.Greater(percentage, 0.9f, "Progress percentage near level up should be > 0.9");
            Assert.LessOrEqual(percentage, 1.0f, "Progress percentage should not exceed 1.0");
        }

        [Test]
        public void Test_CalculateProgressPercentage_ClampedBetween0And1()
        {
            float percentage1 = LevelCalculator.CalculateProgressPercentage(0, 1);
            float percentage2 = LevelCalculator.CalculateProgressPercentage(int.MaxValue, 50);

            Assert.GreaterOrEqual(percentage1, 0f, "Progress percentage should be >= 0");
            Assert.LessOrEqual(percentage1, 1.0f, "Progress percentage should be <= 1");
            Assert.GreaterOrEqual(percentage2, 0f, "Progress percentage should be >= 0");
            Assert.LessOrEqual(percentage2, 1.0f, "Progress percentage should be <= 1");
        }

        [Test]
        public void Test_IsMaxLevel_BelowMax_ReturnsFalse()
        {
            bool isMax = LevelCalculator.IsMaxLevel(50);
            Assert.IsFalse(isMax, "Level 50 should not be max level");
        }

        [Test]
        public void Test_IsMaxLevel_AtMax_ReturnsTrue()
        {
            bool isMax = LevelCalculator.IsMaxLevel(LevelCalculator.GetMaxLevel());
            Assert.IsTrue(isMax, "Max level should return true");
        }

        [Test]
        public void Test_IsMaxLevel_AboveMax_ReturnsTrue()
        {
            bool isMax = LevelCalculator.IsMaxLevel(LevelCalculator.GetMaxLevel() + 10);
            Assert.IsTrue(isMax, "Above max level should return true");
        }

        [Test]
        public void Test_GetMaxLevel_ReturnsPositiveValue()
        {
            int maxLevel = LevelCalculator.GetMaxLevel();
            Assert.Greater(maxLevel, 0, "Max level should be positive");
        }

        [Test]
        public void Test_GetMinLevel_Returns1()
        {
            int minLevel = LevelCalculator.GetMinLevel();
            Assert.AreEqual(1, minLevel, "Min level should be 1");
        }

        [Test]
        public void Test_CalculateXPToNextLevel_AtLevelStart_ReturnsFullRequirement()
        {
            int xpToNext = LevelCalculator.CalculateXPToNextLevel(100, 2);
            int xpRequired = LevelCalculator.CalculateXPForNextLevel(2);
            Assert.AreEqual(xpRequired, xpToNext, "XP to next level at start should equal requirement");
        }

        [Test]
        public void Test_CalculateXPToNextLevel_HalfwayThrough_ReturnsHalfRequirement()
        {
            int xpToNext = LevelCalculator.CalculateXPToNextLevel(175, 2);
            Assert.AreEqual(75, xpToNext, "XP to next level halfway should be 75");
        }

        [Test]
        public void Test_CalculateXPToNextLevel_NearLevelUp_ReturnsSmallValue()
        {
            int xpToNext = LevelCalculator.CalculateXPToNextLevel(249, 2);
            Assert.AreEqual(1, xpToNext, "XP to next level near level up should be 1");
        }

        [Test]
        public void Test_CalculateXPToNextLevel_NeverReturnsNegative()
        {
            int xpToNext = LevelCalculator.CalculateXPToNextLevel(500, 2);
            Assert.GreaterOrEqual(xpToNext, 0, "XP to next level should never be negative");
        }

        [Test]
        public void Test_LevelProgression_Consistency()
        {
            for (int level = 1; level < 10; level++)
            {
                int totalXP = LevelCalculator.CalculateTotalXPForLevel(level);
                int calculatedLevel = LevelCalculator.CalculateLevel(totalXP);
                Assert.AreEqual(level, calculatedLevel, $"Level {level} should be consistent with its total XP");
            }
        }

        [Test]
        public void Test_XPCalculations_MatchExpectedValues()
        {
            Assert.AreEqual(1, LevelCalculator.CalculateLevel(0));
            Assert.AreEqual(1, LevelCalculator.CalculateLevel(99));
            Assert.AreEqual(2, LevelCalculator.CalculateLevel(100));
            Assert.AreEqual(2, LevelCalculator.CalculateLevel(249));
            Assert.AreEqual(3, LevelCalculator.CalculateLevel(250));
        }

        [Test]
        public void Test_ExponentialGrowth_IsCorrect()
        {
            int xp1 = LevelCalculator.CalculateXPForNextLevel(1);
            int xp5 = LevelCalculator.CalculateXPForNextLevel(5);
            int xp10 = LevelCalculator.CalculateXPForNextLevel(10);

            Assert.Greater(xp5, xp1 * 2, "XP should grow exponentially");
            Assert.Greater(xp10, xp5 * 2, "XP should continue growing exponentially");
        }
    }
}
#endif
