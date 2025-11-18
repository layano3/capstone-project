using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Supabase;

/// <summary>
/// Tracks current activity and status in real-time
/// </summary>
public class ActivityTracker : MonoBehaviour
{
    [Header("Config")]
    public SupabaseConfig config;
    
    private static ActivityTracker _instance;
    public static ActivityTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ActivityTracker>();
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
    
    /// <summary>
    /// Updates the student's current activity and status
    /// </summary>
    public void UpdateActivity(string activityName, string status)
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId)) return;
        
        StartCoroutine(SendActivityUpdate(userId, activityName, status));
    }
    
    /// <summary>
    /// Marks activity as completed
    /// </summary>
    public void CompleteActivity(string activityName)
    {
        UpdateActivity(activityName, "completed");
    }
    
    /// <summary>
    /// Sets status when entering a puzzle
    /// </summary>
    public void EnterPuzzle(string puzzleName)
    {
        UpdateActivity(puzzleName, "puzzle");
    }
    
    /// <summary>
    /// Sets status when entering a boss fight
    /// </summary>
    public void EnterBoss(string bossName)
    {
        UpdateActivity(bossName, "boss");
    }
    
    /// <summary>
    /// Sets status when exploring
    /// </summary>
    public void StartExploring(string areaName)
    {
        UpdateActivity(areaName, "exploring");
    }
    
    /// <summary>
    /// Sets offline status
    /// </summary>
    public void SetOffline()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId)) return;
        
        StartCoroutine(SendActivityUpdate(userId, "", "offline"));
    }
    
    IEnumerator SendActivityUpdate(string studentId, string activity, string status)
    {
        // Validate status
        string[] validStatuses = { "exploring", "puzzle", "boss", "completed", "offline" };
        bool isValidStatus = false;
        foreach (var validStatus in validStatuses)
        {
            if (status == validStatus)
            {
                isValidStatus = true;
                break;
            }
        }
        
        if (!isValidStatus)
        {
            Debug.LogWarning($"Invalid activity status: {status}");
            yield break;
        }
        
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string payload = $"{{\"current_activity\":\"{activity}\",\"activity_status\":\"{status}\",\"last_active\":\"{timestamp}\"}}";
        
        using var patchReq = UnityWebRequest.Put(url, payload);
        patchReq.method = "PATCH";
        patchReq.SetRequestHeader("Content-Type", "application/json");
        patchReq.SetRequestHeader("apikey", config.anonKey);
        patchReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        patchReq.SetRequestHeader("Prefer", "return=minimal");
        
        yield return patchReq.SendWebRequest();
        
        if (patchReq.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Activity updated: {activity} ({status})");
        }
        else
        {
            Debug.LogError($"Failed to update activity: {patchReq.error}");
        }
    }
    
    void OnApplicationQuit()
    {
        SetOffline();
    }
    
    [Serializable]
    private class StreakData
    {
        public int streak_days;
    }
}

