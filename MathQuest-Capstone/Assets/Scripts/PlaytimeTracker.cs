using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Supabase;

/// <summary>
/// Tracks playtime and updates the database periodically
/// </summary>
public class PlaytimeTracker : MonoBehaviour
{
    [Header("Config")]
    public SupabaseConfig config;
    public float updateIntervalSeconds = 60f; // Update every 60 seconds
    
    private float sessionStartTime;
    private float lastUpdateTime;
    private int totalMinutesThisSession = 0;
    
    void Start()
    {
        sessionStartTime = Time.time;
        lastUpdateTime = Time.time;
        
        // Update playtime periodically
        InvokeRepeating(nameof(UpdatePlaytime), updateIntervalSeconds, updateIntervalSeconds);
    }
    
    void OnApplicationQuit()
    {
        // Save final playtime before quitting
        UpdatePlaytime();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // App is pausing, save playtime
            UpdatePlaytime();
        }
        else
        {
            // App resuming, reset timer
            lastUpdateTime = Time.time;
        }
    }
    
    void UpdatePlaytime()
    {
        float currentTime = Time.time;
        float minutesSinceLastUpdate = (currentTime - lastUpdateTime) / 60f;
        
        if (minutesSinceLastUpdate < 0.5f) return; // Don't update for less than 30 seconds
        
        totalMinutesThisSession += Mathf.RoundToInt(minutesSinceLastUpdate);
        lastUpdateTime = currentTime;
        
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (!string.IsNullOrEmpty(userId))
        {
            StartCoroutine(SendPlaytimeUpdate(userId, Mathf.RoundToInt(minutesSinceLastUpdate)));
        }
    }
    
    IEnumerator SendPlaytimeUpdate(string studentId, int minutesToAdd)
    {
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        
        // Get current time_played_minutes
        using (var getReq = UnityWebRequest.Get(url + "&select=time_played_minutes"))
        {
            getReq.SetRequestHeader("apikey", config.anonKey);
            getReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
            
            yield return getReq.SendWebRequest();
            
            if (getReq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get current playtime: {getReq.error}");
                yield break;
            }
            
            var json = getReq.downloadHandler.text.Trim('[', ']');
            var data = JsonUtility.FromJson<PlaytimeData>(json);
            
            int newPlaytime = data.time_played_minutes + minutesToAdd;
            
            // Update with new playtime
            string updatePayload = $"{{\"time_played_minutes\":{newPlaytime}}}";
            using var patchReq = UnityWebRequest.Put(url, updatePayload);
            patchReq.method = "PATCH";
            patchReq.SetRequestHeader("Content-Type", "application/json");
            patchReq.SetRequestHeader("apikey", config.anonKey);
            patchReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
            patchReq.SetRequestHeader("Prefer", "return=minimal");
            
            yield return patchReq.SendWebRequest();
            
            if (patchReq.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Updated playtime: +{minutesToAdd} min (total: {newPlaytime} min)");
            }
            else
            {
                Debug.LogError($"Failed to update playtime: {patchReq.error}");
            }
        }
    }
    
    [Serializable]
    private class PlaytimeData
    {
        public int time_played_minutes;
    }
}

