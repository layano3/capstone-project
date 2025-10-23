using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class ProfileController : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text xpText;
    public TMP_Text levelText;
    public Slider xpBar;

    [Header("Supabase Config")]
    public SupabaseConfig config; 

    void Start()
    {
        StartCoroutine(LoadProfileData());
    }

    IEnumerator LoadProfileData()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No user ID found — user not logged in or ID not saved.");
            yield break;
        }

        string url = $"{config.url}/rest/v1/students?id=eq.{userId}&select=name,xp,xp_goal,level";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            Debug.LogError($"Failed to load profile: {req.responseCode} {req.error}");
            yield break;
        }

        var json = req.downloadHandler.text.Trim('[', ']');
        var student = JsonUtility.FromJson<StudentData>(json);

        nameText.text = student.name;
        xpText.text = $"XP: {student.xp}/{student.xp_goal}";
        levelText.text = $"Level {student.level}";
        xpBar.value = (float)student.xp / student.xp_goal;

        Debug.Log($"Profile loaded: {student.name} (XP {student.xp}, Level {student.level})");
    }
}

[Serializable]
public class StudentData
{
    public string name;
    public int xp;
    public int xp_goal;
    public int level;
}
