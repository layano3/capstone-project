using System;
using System.Collections;
using UnityEngine;
using Supabase;

/// <summary>
/// Handles XP rewards with contextual reasons
/// </summary>
public class XPRewardSystem : MonoBehaviour
{
    [Header("Config")]
    public SupabaseConfig config;
    public XPManager xpManager;
    
    [Header("XP Reward Values")]
    public int questCompletionXP = 100;
    public int puzzleSolvedXP = 50;
    public int bossDefeatedXP = 150;
    public int dailyLoginXP = 25;
    public int streakBonusXP = 10; // Per day of streak
    public int helpfulActionXP = 15;
    
    [Header("Audio & VFX (Optional)")]
    [SerializeField] private AudioClip xpGainSound;
    [SerializeField] private AudioClip questCompleteSound;
    [SerializeField] private AudioClip puzzleSolvedSound;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private GameObject xpGainVFX;
    [SerializeField] private GameObject questCompleteVFX;
    [SerializeField] private GameObject puzzleSolvedVFX;
    [SerializeField] private GameObject levelUpVFX;
    [SerializeField] private bool playSounds = true;
    [SerializeField] private bool spawnVFX = true;
    
    private static XPRewardSystem _instance;
    public static XPRewardSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<XPRewardSystem>();
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        if (xpManager == null)
        {
            xpManager = FindObjectOfType<XPManager>();
        }
    }
    
    // Quest-related rewards
    public void RewardQuestCompletion(string questName)
    {
        AwardXP(questCompletionXP, $"Completed quest: {questName}", "QuestSystem");
    }
    
    public void RewardPuzzleSolved(string puzzleName, int difficulty = 1)
    {
        int xp = puzzleSolvedXP * difficulty;
        AwardXP(xp, $"Solved {puzzleName} puzzle", "PuzzleSystem");
    }
    
    public void RewardBossDefeated(string bossName)
    {
        AwardXP(bossDefeatedXP, $"Defeated {bossName}", "BossSystem");
    }
    
    // Daily rewards
    public void RewardDailyLogin()
    {
        AwardXP(dailyLoginXP, "Daily login bonus", "LoginSystem");
    }
    
    public void RewardStreak(int streakDays)
    {
        int xp = streakBonusXP * streakDays;
        AwardXP(xp, $"{streakDays}-day streak bonus!", "StreakSystem");
    }
    
    // Achievement-based rewards
    public void RewardFirstCompletion(string achievementName)
    {
        AwardXP(75, $"First time: {achievementName}", "AchievementSystem");
    }
    
    public void RewardPerfectScore(string activityName)
    {
        AwardXP(100, $"Perfect score on {activityName}!", "PerformanceSystem");
    }
    
    // Social/helpful actions
    public void RewardHelpfulAction(string action)
    {
        AwardXP(helpfulActionXP, action, "SocialSystem");
    }
    
    // Performance-based
    public void RewardAccuracy(int accuracyPercent, string activityName)
    {
        int xp = Mathf.RoundToInt(accuracyPercent * 0.5f); // Up to 50 XP for 100%
        if (xp > 0)
        {
            AwardXP(xp, $"{accuracyPercent}% accuracy on {activityName}", "PerformanceSystem");
        }
    }
    
    public void RewardSpeed(string activityName, string speedCategory)
    {
        int xp = speedCategory == "fast" ? 30 : speedCategory == "very-fast" ? 50 : 20;
        AwardXP(xp, $"Quick completion: {activityName}", "SpeedSystem");
    }
    
    // Custom rewards
    public void RewardCustom(int xpAmount, string reason, string source = "GameSystem")
    {
        AwardXP(xpAmount, reason, source);
    }
    
    // Core award method
    private void AwardXP(int amount, string reason, string updatedBy)
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("Cannot award XP - no user logged in");
            return;
        }
        
        if (xpManager == null)
        {
            Debug.LogError("XPManager reference is null!");
            return;
        }
        
        StartCoroutine(xpManager.AddXpEvent(userId, amount, reason, updatedBy, (error) =>
        {
            if (error == null)
            {
                Debug.Log($"✅ Awarded {amount} XP: {reason}");
                
                // Play sound effects and spawn VFX
                PlayRewardEffects(amount, reason, updatedBy);
                
                // Trigger behaviour analysis after significant XP gains
                if (amount >= 50)
                {
                    var analyzer = FindObjectOfType<BehaviourAnalyzer>();
                    if (analyzer != null)
                    {
                        StartCoroutine(analyzer.AnalyzeAndUpdateBehaviour(userId));
                    }
                }
            }
        }));
    }
    
    private void PlayRewardEffects(int amount, string reason, string source)
    {
        // Find player position for VFX spawning
        Vector3 playerPosition = GetPlayerPosition();
        
        // Determine which sound and VFX to use based on source
        AudioClip soundToPlay = null;
        GameObject vfxToSpawn = null;
        
        switch (source)
        {
            case "QuestSystem":
                soundToPlay = questCompleteSound;
                vfxToSpawn = questCompleteVFX;
                break;
            case "PuzzleSystem":
                soundToPlay = puzzleSolvedSound;
                vfxToSpawn = puzzleSolvedVFX;
                break;
            default:
                soundToPlay = xpGainSound;
                vfxToSpawn = xpGainVFX;
                break;
        }
        
        // Play sound effect
        if (playSounds && soundToPlay != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXOneShot(soundToPlay, playerPosition, 0.7f);
        }
        
        // Spawn VFX
        if (spawnVFX && vfxToSpawn != null && VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnVFX(vfxToSpawn, playerPosition, Quaternion.identity, null, 3f);
        }
    }
    
    private Vector3 GetPlayerPosition()
    {
        // Try to find player/camera position
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position + Vector3.up * 1.5f; // Slightly above player
        }
        
        var camera = Camera.main;
        if (camera != null)
        {
            return camera.transform.position;
        }
        
        return Vector3.zero;
    }
}

