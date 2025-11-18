using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Supabase;

/// <summary>
/// Manages daily login streaks
/// </summary>
public class StreakManager : MonoBehaviour
{
    [Header("Config")]
    public SupabaseConfig config;
    
    private const string LAST_LOGIN_DATE_KEY = "LastLoginDate";
    
    void Start()
    {
        CheckAndUpdateStreak();
    }
    
    void CheckAndUpdateStreak()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId)) return;
        
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastLoginDate = PlayerPrefs.GetString(LAST_LOGIN_DATE_KEY, "");
        
        if (lastLoginDate == today)
        {
            // Already logged in today, no update needed
            Debug.Log("Already logged in today - streak maintained");
            return;
        }
        
        StartCoroutine(UpdateStreakInDatabase(userId, lastLoginDate, today));
    }
    
    IEnumerator UpdateStreakInDatabase(string studentId, string lastLogin, string today)
    {
        // Get current streak
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}&select=streak_days";
        using var getReq = UnityWebRequest.Get(url);
        getReq.SetRequestHeader("apikey", config.anonKey);
        getReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        
        yield return getReq.SendWebRequest();
        
        if (getReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to get streak: {getReq.error}");
            yield break;
        }
        
        var json = getReq.downloadHandler.text.Trim('[', ']');
        var data = JsonUtility.FromJson<StreakData>(json);
        
        int newStreak = CalculateNewStreak(data.streak_days, lastLogin, today);
        
        // Update streak in database
        string updateUrl = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        string payload = $"{{\"streak_days\":{newStreak},\"last_active\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"}}";
        
        using var patchReq = UnityWebRequest.Put(updateUrl, payload);
        patchReq.method = "PATCH";
        patchReq.SetRequestHeader("Content-Type", "application/json");
        patchReq.SetRequestHeader("apikey", config.anonKey);
        patchReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        patchReq.SetRequestHeader("Prefer", "return=minimal");
        
        yield return patchReq.SendWebRequest();
        
        if (patchReq.result == UnityWebRequest.Result.Success)
        {
            PlayerPrefs.SetString(LAST_LOGIN_DATE_KEY, today);
            PlayerPrefs.Save();
            Debug.Log($"Streak updated: {newStreak} days");
        }
        else
        {
            Debug.LogError($"Failed to update streak: {patchReq.error}");
        }
    }
    
    int CalculateNewStreak(int currentStreak, string lastLoginDate, string today)
    {
        if (string.IsNullOrEmpty(lastLoginDate))
        {
            // First login ever
            return 1;
        }
        
        if (!DateTime.TryParse(lastLoginDate, out DateTime lastDate))
        {
            // Invalid date, start fresh
            return 1;
        }
        
        if (!DateTime.TryParse(today, out DateTime todayDate))
        {
            return currentStreak; // Should never happen
        }
        
        TimeSpan diff = todayDate - lastDate;
        
        if (diff.TotalDays == 1)
        {
            // Logged in yesterday, increment streak
            return currentStreak + 1;
        }
        else if (diff.TotalDays > 1)
        {
            // Missed a day, reset streak
            return 1;
        }
        else
        {
            // Same day (shouldn't happen due to check above)
            return currentStreak;
        }
    }
    
    [Serializable]
    private class StreakData
    {
        public int streak_days;
    }
}

