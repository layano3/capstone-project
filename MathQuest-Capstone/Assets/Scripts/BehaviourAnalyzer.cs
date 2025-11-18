using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Supabase;

/// <summary>
/// Analyzes student performance and updates behaviour status
/// </summary>
public class BehaviourAnalyzer : MonoBehaviour
{
    [Header("Config")]
    public SupabaseConfig config;
    
    [Header("Behaviour Thresholds")]
    [Tooltip("Days of inactivity before marking as at-risk")]
    public int inactiveDaysThreshold = 7;
    
    [Tooltip("Minimum XP per day to stay on-track")]
    public int minXpPerDay = 50;
    
    [Tooltip("Streak days needed to be considered on-track")]
    public int minStreakForOnTrack = 3;
    
    /// <summary>
    /// Analyzes and updates behaviour based on student activity
    /// </summary>
    public IEnumerator AnalyzeAndUpdateBehaviour(string studentId)
    {
        // Get student data
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        using var getReq = UnityWebRequest.Get(url);
        getReq.SetRequestHeader("apikey", config.anonKey);
        getReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        
        yield return getReq.SendWebRequest();
        
        if (getReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to get student data: {getReq.error}");
            yield break;
        }
        
        var json = getReq.downloadHandler.text.Trim('[', ']');
        var student = JsonUtility.FromJson<Student>(json);
        
        // Calculate new behaviour status
        string newBehaviour = CalculateBehaviour(student);
        
        // Only update if changed
        if (newBehaviour != student.behaviour)
        {
            yield return UpdateBehaviourInDatabase(studentId, newBehaviour);
        }
    }
    
    string CalculateBehaviour(Student student)
    {
        // Parse last_active timestamp
        DateTime lastActive;
        if (!DateTime.TryParse(student.last_active, out lastActive))
        {
            lastActive = DateTime.UtcNow.AddDays(-100); // Assume very old if parse fails
        }
        
        TimeSpan timeSinceActive = DateTime.UtcNow - lastActive;
        
        // Check for at-risk conditions
        if (timeSinceActive.TotalDays > inactiveDaysThreshold)
        {
            return "at-risk";
        }
        
        if (student.streak_days == 0)
        {
            return "at-risk";
        }
        
        // Check for needs-support conditions
        if (student.streak_days < minStreakForOnTrack)
        {
            return "needs-support";
        }
        
        // Calculate average XP per day
        DateTime accountCreated;
        if (DateTime.TryParse(student.created_at, out accountCreated))
        {
            double daysSinceCreation = Math.Max(1, (DateTime.UtcNow - accountCreated).TotalDays);
            double avgXpPerDay = student.xp / daysSinceCreation;
            
            if (avgXpPerDay < minXpPerDay * 0.5) // Less than half expected
            {
                return "needs-support";
            }
        }
        
        // Default: on-track
        return "on-track";
    }
    
    IEnumerator UpdateBehaviourInDatabase(string studentId, string newBehaviour)
    {
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}";
        string payload = $"{{\"behaviour\":\"{newBehaviour}\"}}";
        
        using var patchReq = UnityWebRequest.Put(url, payload);
        patchReq.method = "PATCH";
        patchReq.SetRequestHeader("Content-Type", "application/json");
        patchReq.SetRequestHeader("apikey", config.anonKey);
        patchReq.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        patchReq.SetRequestHeader("Prefer", "return=minimal");
        
        yield return patchReq.SendWebRequest();
        
        if (patchReq.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Behaviour updated to: {newBehaviour}");
        }
        else
        {
            Debug.LogError($"Failed to update behaviour: {patchReq.error}");
        }
    }
}

