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
    
    void Awake()
    {
        // Try to find config if not set
        if (config == null)
        {
            // Try to get from GameSessionManager first
            var gameSession = FindObjectOfType<GameSessionManager>();
            if (gameSession != null && gameSession.config != null)
            {
                config = gameSession.config;
                Debug.Log("PlaytimeTracker: Found config from GameSessionManager");
            }
            else
            {
                // Try to find SupabaseConfig in resources
                var configs = Resources.FindObjectsOfTypeAll<SupabaseConfig>();
                if (configs != null && configs.Length > 0)
                {
                    config = configs[0];
                    Debug.Log("PlaytimeTracker: Found config from Resources");
                }
            }
        }
    }
    
    void Start()
    {
        // Ensure config is set
        if (config == null)
        {
            Debug.LogError("PlaytimeTracker: SupabaseConfig is not set! Playtime tracking disabled. Please assign config in Inspector or ensure GameSessionManager has config set.");
            return;
        }
        
        // Check for user ID
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("PlaytimeTracker: No user ID found. Playtime tracking will not work until user logs in.");
        }
        else
        {
            Debug.Log($"PlaytimeTracker: Tracking playtime for user: {userId}");
        }
        
        sessionStartTime = Time.time;
        lastUpdateTime = Time.time;
        
        Debug.Log($"PlaytimeTracker: Started tracking. Updates every {updateIntervalSeconds} seconds.");
        
        // Update playtime periodically
        InvokeRepeating(nameof(UpdatePlaytime), updateIntervalSeconds, updateIntervalSeconds);
    }
    
    /// <summary>
    /// Manually trigger a playtime update (useful for testing)
    /// </summary>
    public void ForceUpdate()
    {
        UpdatePlaytime();
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
        if (config == null)
        {
            Debug.LogWarning("PlaytimeTracker: Config is null, cannot update playtime.");
            return;
        }
        
        float currentTime = Time.time;
        float minutesSinceLastUpdate = (currentTime - lastUpdateTime) / 60f;
        
        if (minutesSinceLastUpdate < 0.5f) return; // Don't update for less than 30 seconds
        
        totalMinutesThisSession += Mathf.RoundToInt(minutesSinceLastUpdate);
        lastUpdateTime = currentTime;
        
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("PlaytimeTracker: No user ID found, skipping playtime update.");
            return;
        }
        
        int minutesToAdd = Mathf.RoundToInt(minutesSinceLastUpdate);
        Debug.Log($"PlaytimeTracker: Updating playtime - adding {minutesToAdd} minutes");
        StartCoroutine(SendPlaytimeUpdate(userId, minutesToAdd));
    }
    
    IEnumerator SendPlaytimeUpdate(string studentId, int minutesToAdd)
    {
        if (config == null)
        {
            Debug.LogError("PlaytimeTracker: Config is null, cannot send playtime update.");
            yield break;
        }
        
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        
        // Get current time_played_minutes
        using (var getReq = UnityWebRequest.Get(url + "&select=time_played_minutes"))
        {
            getReq.SetRequestHeader("apikey", config.anonKey);
            getReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
            
            yield return getReq.SendWebRequest();
            
            if (getReq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PlaytimeTracker: Failed to get current playtime: {getReq.responseCode} - {getReq.error}");
                if (getReq.downloadHandler != null && !string.IsNullOrEmpty(getReq.downloadHandler.text))
                {
                    Debug.LogError($"Response: {getReq.downloadHandler.text}");
                }
                yield break;
            }
            
            var json = getReq.downloadHandler.text.Trim('[', ']');
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("PlaytimeTracker: Empty response when getting playtime data.");
                yield break;
            }
            
            var data = JsonUtility.FromJson<PlaytimeData>(json);
            
            if (data == null)
            {
                Debug.LogError($"PlaytimeTracker: Failed to parse playtime data. JSON: {json}");
                yield break;
            }
            
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
                Debug.Log($"PlaytimeTracker: Successfully updated playtime: +{minutesToAdd} min (total: {newPlaytime} min)");
            }
            else
            {
                Debug.LogError($"PlaytimeTracker: Failed to update playtime: {patchReq.responseCode} - {patchReq.error}");
                if (patchReq.downloadHandler != null && !string.IsNullOrEmpty(patchReq.downloadHandler.text))
                {
                    Debug.LogError($"Response: {patchReq.downloadHandler.text}");
                }
            }
        }
    }
    
    [Serializable]
    private class PlaytimeData
    {
        public int time_played_minutes;
    }
}

