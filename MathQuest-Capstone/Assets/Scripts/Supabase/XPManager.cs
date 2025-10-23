using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class XPManager : MonoBehaviour
{
    [Header("Supabase Config")]
    public SupabaseConfig config; // Drag your SupabaseConfig ScriptableObject here

    public IEnumerator GetStudentData(string studentId, Action<Student, string> callback)
    {
        string url = $"{config.url}/rest/v1/students?id=eq.{studentId}&select=*";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            callback(null, $"Error {req.responseCode}: {req.error}");
            yield break;
        }

        string json = req.downloadHandler.text.Trim('[', ']');
        var student = JsonUtility.FromJson<Student>(json);
        callback(student, null);
    }

    public IEnumerator AddXpEvent(string studentId, int delta, string reason, string updatedBy, Action<string> callback = null)
    {
        string url = $"{config.url}/rest/v1/rpc/add_xp_event";

        // Build JSON payload for your RPC
        string payload = JsonUtility.ToJson(new XPEventPayload
        {
            p_student_id = studentId,
            p_delta = delta,
            p_reason = reason,
            p_updated_by = updatedBy
        });

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            Debug.LogError($"Failed to add XP: {req.responseCode} {req.error}");
            callback?.Invoke($"Error {req.responseCode}: {req.error}");
            yield break;
        }

        Debug.Log($"Added {delta} XP for reason: {reason}");
        callback?.Invoke(null);
    }

    [Serializable]
    private class XPEventPayload
    {
        public string p_student_id;
        public int p_delta;
        public string p_reason;
        public string p_updated_by;
    }
}
