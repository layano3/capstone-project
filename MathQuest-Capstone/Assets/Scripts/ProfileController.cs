using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Supabase;

public class ProfileController : MonoBehaviour
{
    [Header("Basic Info UI")]
    public TMP_Text nameText;
    public TMP_Text xpText;
    public TMP_Text levelText;
    public Slider xpBar;

    [Header("Tab Buttons")]
    public Button statsButton;
    public Button achievementButton;
    
    [Header("Content Panels")]
    public GameObject statsPanel;
    public GameObject achievementPanel;
    
    [Header("Stats Content UI")]
    public TMP_Text gradeText;
    public TMP_Text timePlayedText;
    public TMP_Text streakText;
    public TMP_Text behaviourText;
    public TMP_Text activityStatusText;
    public TMP_Text lastActiveText;
    
    [Header("Achievement Content UI")]
    public Transform xpEventsContainer;
    public GameObject xpEventRowPrefab; // Prefab for XP event row

    [Header("Supabase Config")]
    public SupabaseConfig config; 

    private StudentData fullStudentData;

    void Start()
    {
        // Set up tab buttons
        if (statsButton != null)
            statsButton.onClick.AddListener(() => SwitchTab(true));
        if (achievementButton != null)
            achievementButton.onClick.AddListener(() => SwitchTab(false));
        
        // Load profile data
        StartCoroutine(LoadProfileData());
    }

    void SwitchTab(bool showStats)
    {
        if (statsPanel != null)
            statsPanel.SetActive(showStats);
        if (achievementPanel != null)
            achievementPanel.SetActive(!showStats);
    }

    IEnumerator LoadProfileData()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No user ID found — user not logged in or ID not saved.");
            yield break;
        }

        // Load all student data
        string url = $"{config.url}/rest/v1/students?id=eq.{userId}";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        req.SetRequestHeader("Prefer", "return=representation");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            Debug.LogError($"Failed to load profile: {req.responseCode} {req.error}");
            yield break;
        }

        var json = req.downloadHandler.text.Trim('[', ']');
        fullStudentData = JsonUtility.FromJson<StudentData>(json);

        // Update basic info
        if (nameText != null)
            nameText.text = fullStudentData.name;
        if (xpText != null)
            xpText.text = $"XP: {fullStudentData.xp}/{fullStudentData.xp_goal}";
        if (levelText != null)
            levelText.text = $"Level {fullStudentData.level}";
        if (xpBar != null)
            xpBar.value = (float)fullStudentData.xp / (float)fullStudentData.xp_goal;

        // Update stats panel
        UpdateStatsPanel();
        
        // Load and display XP events
        yield return StartCoroutine(LoadXPEvents(userId));
        
        Debug.Log($"Profile loaded: {fullStudentData.name} (XP {fullStudentData.xp}, Level {fullStudentData.level})");
    }

    void UpdateStatsPanel()
    {
        if (gradeText != null && fullStudentData != null)
            gradeText.text = $"Grade: {fullStudentData.grade}";
        
        if (timePlayedText != null && fullStudentData != null)
        {
            int hours = fullStudentData.time_played_minutes / 60;
            int minutes = fullStudentData.time_played_minutes % 60;
            timePlayedText.text = $"Time Played: {hours}h {minutes}m";
        }
        
        if (streakText != null && fullStudentData != null)
            streakText.text = $"Streak: {fullStudentData.streak_days} days";
        
        if (behaviourText != null && fullStudentData != null)
            behaviourText.text = $"Behaviour: {fullStudentData.behaviour}";
        
        if (activityStatusText != null && fullStudentData != null)
            activityStatusText.text = $"Status: {fullStudentData.activity_status}";
        
        if (lastActiveText != null && fullStudentData != null)
            lastActiveText.text = $"Last Active: {FormatTimestamp(fullStudentData.last_active)}";
    }

    IEnumerator LoadXPEvents(string studentId)
    {
        string url = $"{config.url}/rest/v1/xp_events?student_id=eq.{studentId}&order=created_at.desc&limit=20";
        Debug.Log($"Loading XP events from: {url}");
        
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            Debug.LogError($"Failed to load XP events: {req.responseCode} {req.error}");
            yield break;
        }

        string jsonResponse = req.downloadHandler.text;
        Debug.Log($"XP Events Response: {jsonResponse}");
        
        // Handle empty array or no data
        if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "[]")
        {
            Debug.LogWarning("No XP events found for this student");
            
            // Show placeholder message
            if (xpEventsContainer != null)
            {
                GameObject placeholder = new GameObject("NoDataText");
                placeholder.transform.SetParent(xpEventsContainer, false);
                
                var rect = placeholder.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.sizeDelta = new Vector2(0, 50);
                
                var tmp = placeholder.AddComponent<TextMeshProUGUI>();
                tmp.text = "No XP events yet. Complete activities to earn XP!";
                tmp.fontSize = 18;
                tmp.color = new Color(1, 1, 1, 0.7f);
                tmp.alignment = TextAlignmentOptions.Center;
            }
            yield break;
        }
        
        // Handle array response
        if (jsonResponse.StartsWith("["))
        {
            // Parse array of events
            var jsonWrapper = "{\"events\":" + jsonResponse + "}";
            var wrapper = JsonUtility.FromJson<XPEventsWrapper>(jsonWrapper);
            
            Debug.Log($"Parsed {wrapper.events?.Length ?? 0} XP events");
            
            if (wrapper.events != null && xpEventsContainer != null)
            {
                // Clear existing entries
                foreach (Transform child in xpEventsContainer)
                {
                    if (child.gameObject.name != "HeaderRow") // Keep header
                        Destroy(child.gameObject);
                }

                // Add entries
                foreach (var xpEvent in wrapper.events)
                {
                    DisplayXPEvent(xpEvent);
                }
                
                Debug.Log($"Displayed {wrapper.events.Length} XP events in UI");
            }
            else if (xpEventsContainer == null)
            {
                Debug.LogError("xpEventsContainer is null! Make sure it's assigned in the Inspector.");
            }
        }
    }

    void DisplayXPEvent(XPEventData xpEvent)
    {
        Debug.Log($"Creating XP event row for: {xpEvent.reason}");
        
        // If no prefab, create a simple row dynamically
        GameObject row;
        if (xpEventRowPrefab != null)
        {
            row = Instantiate(xpEventRowPrefab, xpEventsContainer);
        }
        else
        {
            // Create basic row manually
            row = new GameObject("XPEventRow");
            row.AddComponent<RectTransform>(); // Must add before SetParent
            row.transform.SetParent(xpEventsContainer, false);
            
            var rectTransform = row.GetComponent<RectTransform>();
            
            // Add Layout Element to control height (this is what the Vertical Layout Group uses)
            var rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.minHeight = 80;
            rowLayout.preferredHeight = 80;
            rowLayout.flexibleHeight = 0;
            rowLayout.minWidth = 1600;
            
            // Add Horizontal Layout Group for the cells
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 120;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(60, 60, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            
            CreateTextCell(row, "+" + xpEvent.delta, 0.15f);
            CreateTextCell(row, xpEvent.reason, 0.6f);
            CreateTextCell(row, FormatTimestamp(xpEvent.created_at), 0.25f);
            
            Debug.Log($"Row created: {row.name}, parent: {row.transform.parent.name}");
        }

        // If prefab exists, configure its text elements
        // This assumes the prefab has TMP_Text components named appropriately
        var texts = row.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 3)
        {
            texts[0].text = "+" + xpEvent.delta;
            texts[1].text = xpEvent.reason;
            texts[2].text = FormatTimestamp(xpEvent.created_at);
        }
    }

    void CreateTextCell(GameObject parent, string text, float flexWidth)
    {
        GameObject cell = new GameObject("Cell_" + text.Substring(0, Math.Min(10, text.Length)));
        cell.AddComponent<RectTransform>();
        cell.transform.SetParent(parent.transform, false);
        
        var tmp = cell.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 40;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.enableWordWrapping = false;
        
        // Add text shadow for better readability
        var shadow = cell.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.7f);
        shadow.effectDistance = new Vector2(2, -2);
        
        // Set layout properties
        var layoutElement = cell.AddComponent<LayoutElement>();
        layoutElement.flexibleWidth = flexWidth;
        layoutElement.minWidth = 150;
        layoutElement.preferredHeight = 60;
    }

    string FormatTimestamp(string timestamp)
    {
        if (string.IsNullOrEmpty(timestamp))
            return "N/A";
        
        // Parse ISO 8601 timestamp
        if (DateTime.TryParse(timestamp, out DateTime dateTime))
        {
            return dateTime.ToString("MM/dd/yyyy HH:mm");
        }
        return timestamp;
    }
}

[Serializable]
public class StudentData
{
    public string id;
    public string name;
    public string grade;
    public string avatar_color;
    public string last_active;
    public string current_activity;
    public string activity_status;
    public int xp;
    public int xp_goal;
    public int time_played_minutes;
    public int streak_days;
    public string behaviour;
    public string next_checkpoint;
    public string guardians;
    public string created_at;
    public string updated_at;
    public int level;
}

[Serializable]
public class XPEventData
{
    public string id;
    public string student_id;
    public int delta;
    public string reason;
    public string updated_by;
    public string created_at;
}

[Serializable]
public class XPEventsWrapper
{
    public XPEventData[] events;
}
