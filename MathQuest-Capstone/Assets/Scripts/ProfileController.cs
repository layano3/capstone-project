using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public UnityEngine.UI.Image avatarSilhouette;

    [Header("Tab Buttons")]
    public Button statsButton;
    public Button achievementButton;
    public Button captainLogButton;
    
    [Header("Content Panels")]
    public GameObject statsPanel;
    public GameObject achievementPanel;
    public GameObject captainLogPanel;
    
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

    [Header("Captain's Log UI")]
    public Transform captainLogContainer;
    public GameObject logEntryPrefab; // Prefab for log entry

    [Header("Supabase Config")]
    public SupabaseConfig config; 

    private StudentData fullStudentData;

    void Start()
    {
        // Set up tab buttons
        if (statsButton != null)
            statsButton.onClick.AddListener(() => SwitchTab(TabType.Stats));
        if (achievementButton != null)
            achievementButton.onClick.AddListener(() => SwitchTab(TabType.Achievements));
        if (captainLogButton != null)
            captainLogButton.onClick.AddListener(() => SwitchTab(TabType.CaptainsLog));
        
        // Add shadows to all text elements
        AddShadowsToTextElements();
        
        // Add shadows to buttons and panels
        AddShadowsToUIElements();
        
        // Load profile data
        StartCoroutine(LoadProfileData());
    }

    void AddShadowsToTextElements()
    {
        // Add shadows to all text elements
        TMP_Text[] textElements = {
            nameText, xpText, levelText, gradeText, timePlayedText, 
            streakText, behaviourText, activityStatusText, lastActiveText
        };
        
        foreach (var text in textElements)
        {
            if (text != null && text.GetComponent<UnityEngine.UI.Shadow>() == null)
            {
                var shadow = text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.75f);
                shadow.effectDistance = new Vector2(3, -3);
                shadow.useGraphicAlpha = true;
            }
        }
        
        // Add shadows to any text in XP event rows (will be added when rows are created)
    }
    
    void AddShadowsToUIElements()
    {
        // Add shadows to XP bar
        if (xpBar != null)
        {
            var sliderImage = xpBar.fillRect?.GetComponent<UnityEngine.UI.Image>();
            if (sliderImage != null && sliderImage.GetComponent<UnityEngine.UI.Shadow>() == null)
            {
                var shadow = sliderImage.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(3, -3);
                shadow.useGraphicAlpha = true;
            }
        }
        
        // Add shadows to buttons (will be added via scene file)
    }

    enum TabType
    {
        Stats,
        Achievements,
        CaptainsLog
    }

    void SwitchTab(TabType tab)
    {
        // Hide all panels first
        if (statsPanel != null)
            statsPanel.SetActive(tab == TabType.Stats);
        if (achievementPanel != null)
            achievementPanel.SetActive(tab == TabType.Achievements);
        if (captainLogPanel != null)
            captainLogPanel.SetActive(tab == TabType.CaptainsLog);
    }

    IEnumerator LoadProfileData()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No user ID found - user not logged in or ID not saved.");
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

        // Avatar silhouette color is not updated from database - keeping default black color

        // Update stats panel
        UpdateStatsPanel();
        
        // Load and display XP events
        yield return StartCoroutine(LoadXPEvents(userId));
        
        // Generate and display captain's log entries from student data
        LoadCaptainLogData();
        
        Debug.Log($"Profile loaded: {fullStudentData.name} (XP {fullStudentData.xp}, Level {fullStudentData.level})");
    }

    // Avatar color update disabled - avatars are not stored in database yet
    // When avatars are implemented, uncomment and use UpdateAvatarSilhouette() method below

    void UpdateStatsPanel()
    {
        if (statsPanel == null || fullStudentData == null)
            return;

        // Clear existing children
        foreach (Transform child in statsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Configure container for full expansion
        var containerRect = statsPanel.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
        }

        // Ensure container has VerticalLayoutGroup configured properly
        var verticalLayout = statsPanel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = statsPanel.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        }
        
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childControlHeight = false;
        verticalLayout.spacing = 120; // Spacing between entries
        verticalLayout.padding = new RectOffset(30, 20, 300, 20);

        // Get font from gradeText for consistency
        var statsFont = gradeText != null ? gradeText.font : null;
        var statsFontMaterial = gradeText != null ? gradeText.fontSharedMaterial : null;

        // Create stat entries with same layout as Captain's Log
        CreateStatEntry("Grade", $"{fullStudentData.grade}", statsFont, statsFontMaterial);
        
        int hours = fullStudentData.time_played_minutes / 60;
        int minutes = fullStudentData.time_played_minutes % 60;
        CreateStatEntry("Time Played", $"{hours}h {minutes}m", statsFont, statsFontMaterial);
        
        CreateStatEntry("Streak", $"{fullStudentData.streak_days} days", statsFont, statsFontMaterial);
        CreateStatEntry("Behaviour", fullStudentData.behaviour, statsFont, statsFontMaterial);
        CreateStatEntry("Status", fullStudentData.activity_status, statsFont, statsFontMaterial);
        CreateStatEntry("Last Active", FormatTimestamp(fullStudentData.last_active), statsFont, statsFontMaterial);
    }

    void CreateStatEntry(string label, string value, TMPro.TMP_FontAsset font, Material fontMaterial)
    {
        // Create stat row
        GameObject statRow = new GameObject($"StatEntry_{label}");
        statRow.transform.SetParent(statsPanel.transform, false);

        // Set up row layout
        var rowRect = statRow.AddComponent<RectTransform>();
        var rowLayout = statRow.AddComponent<LayoutElement>();
        
        rowLayout.minHeight = 150;
        rowLayout.preferredHeight = 0; // Let content determine height
        rowLayout.flexibleHeight = 1;
        rowLayout.flexibleWidth = 1;
        
        // Ensure row expands to full width
        rowRect.anchorMin = new Vector2(0, 0);
        rowRect.anchorMax = new Vector2(1, 0);
        rowRect.sizeDelta = Vector2.zero;
        rowRect.pivot = new Vector2(0.5f, 1f);

        // Add horizontal layout for label and content
        var layout = statRow.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.padding = new RectOffset(20, 20, 15, 15);
        layout.childControlWidth = true;
        layout.childForceExpandWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        // Create label text (heading)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(statRow.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;
        
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        
        // Explicitly set font - MUST be set before other properties (same as DisplayLogEntry)
        if (font != null)
        {
            labelText.font = font;
            labelText.fontSharedMaterial = fontMaterial;
        }
        else if (gradeText != null && gradeText.font != null)
        {
            // Explicit fallback: use Stats font directly
            labelText.font = gradeText.font;
            labelText.fontSharedMaterial = gradeText.fontSharedMaterial;
        }
        
        labelText.text = label + ":";
        labelText.color = new Color(1f, 0.9f, 0.6f, 1f); // Golden color for headings
        labelText.fontSize = 52; // Increased font size
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.verticalAlignment = VerticalAlignmentOptions.Top;
        labelText.enableWordWrapping = true;
        labelText.overflowMode = TextOverflowModes.Overflow;
        
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 400; // Increased to prevent truncation
        labelLayout.minWidth = 380; // Increased minimum width
        labelLayout.flexibleWidth = 0;
        labelLayout.flexibleHeight = 1;
        
        // Add shadow to label
        var labelShadow = labelObj.AddComponent<UnityEngine.UI.Shadow>();
        labelShadow.effectColor = new Color(0, 0, 0, 0.75f);
        labelShadow.effectDistance = new Vector2(2, -2);
        labelShadow.useGraphicAlpha = true;

        // Create content text
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(statRow.transform, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;
        
        var contentText = contentObj.AddComponent<TextMeshProUGUI>();
        
        // Explicitly set font - MUST be set before other properties (same as DisplayLogEntry)
        if (font != null)
        {
            contentText.font = font;
            contentText.fontSharedMaterial = fontMaterial;
        }
        else if (gradeText != null && gradeText.font != null)
        {
            // Explicit fallback: use Stats font directly
            contentText.font = gradeText.font;
            contentText.fontSharedMaterial = gradeText.fontSharedMaterial;
        }
        
        contentText.text = value;
        contentText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        contentText.fontSize = 48; // Increased font size
        contentText.fontStyle = FontStyles.Normal;
        contentText.alignment = TextAlignmentOptions.Left;
        contentText.verticalAlignment = VerticalAlignmentOptions.Top;
        contentText.enableWordWrapping = true;
        contentText.overflowMode = TextOverflowModes.Overflow;
        contentText.lineSpacing = 8;
        
        var contentLayout = contentObj.AddComponent<LayoutElement>();
        contentLayout.flexibleWidth = 1; // Match Captain's Log
        contentLayout.preferredWidth = 0; // Match Captain's Log
        contentLayout.minWidth = 0; // Match Captain's Log - let flexible width handle it
        contentLayout.flexibleHeight = 1; // Match Captain's Log
        
        // Add shadow to content (match Captain's Log)
        var contentShadow = contentObj.AddComponent<UnityEngine.UI.Shadow>();
        contentShadow.effectColor = new Color(0, 0, 0, 0.75f); // Match Captain's Log shadow
        contentShadow.effectDistance = new Vector2(2, -2);
        contentShadow.useGraphicAlpha = true;
    }

    IEnumerator LoadXPEvents(string studentId)
    {
        // Ensure container is properly configured for full width expansion
        if (xpEventsContainer != null)
        {
            // Ensure container has VerticalLayoutGroup with proper settings
            var verticalLayout = xpEventsContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = xpEventsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            verticalLayout.childForceExpandWidth = true; // Enable width expansion for rows
            verticalLayout.childControlWidth = true; // Control child widths
            verticalLayout.spacing = 12; // Spacing between cards
            verticalLayout.padding = new RectOffset(10, 10, 50, -50); // More top padding to push cards below tabs
            
            // Ensure RectTransform stretches to fill parent
            var containerRect = xpEventsContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 1);
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
            }
            
            // Also configure the parent achievement panel to center entries vertically
            if (achievementPanel != null)
            {
                var panelLayout = achievementPanel.GetComponent<VerticalLayoutGroup>();
                if (panelLayout != null)
                {
                    panelLayout.padding = new RectOffset(5, 5, 250, 120); // Top and bottom padding to center entries
                    panelLayout.childAlignment = TextAnchor.UpperCenter; // Align to top-center
                }
                
                var panelRect = achievementPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    // Ensure panel fills its parent
                    panelRect.anchorMin = new Vector2(0, 0);
                    panelRect.anchorMax = new Vector2(1, 1);
                    panelRect.sizeDelta = Vector2.zero;
                    panelRect.anchoredPosition = Vector2.zero;
                }
            }
        }
        
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
                    Destroy(child.gameObject);
                }
                
                // Add entries as cards (no header needed) - limit to 7 entries
                int maxEntries = 7;
                int entriesToShow = Mathf.Min(wrapper.events.Length, maxEntries);
                for (int i = 0; i < entriesToShow; i++)
                {
                    CreateXPEventCard(wrapper.events[i]);
                }
                
                Debug.Log($"Displayed {entriesToShow} of {wrapper.events.Length} XP events in UI (limited to {maxEntries})");
            }
            else if (xpEventsContainer == null)
            {
                Debug.LogError("xpEventsContainer is null! Make sure it's assigned in the Inspector.");
            }
        }
    }

    void CreateXPEventCard(XPEventData xpEvent)
    {
        Debug.Log($"Creating XP event card for: {xpEvent.reason}");
        
        // Create card container
        GameObject card = new GameObject("XPEventCard");
        var cardRect = card.AddComponent<RectTransform>();
        card.transform.SetParent(xpEventsContainer, false);
        
        // Set card RectTransform to stretch horizontally
        cardRect.anchorMin = new Vector2(0, 0);
        cardRect.anchorMax = new Vector2(1, 0);
        cardRect.sizeDelta = new Vector2(0, 100); // Taller cards for better spacing
        cardRect.anchoredPosition = Vector2.zero;
        
        // Add card background with rounded appearance (using padding for effect)
        var cardBg = card.AddComponent<UnityEngine.UI.Image>();
        cardBg.color = new Color(0.25f, 0.25f, 0.25f, 0.6f); // Grey card background (no blue tint)
        
        // Add Layout Element
        var cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.minHeight = 100;
        cardLayout.preferredHeight = 100;
        cardLayout.flexibleHeight = 0;
        cardLayout.flexibleWidth = 1;
        
        // Add Horizontal Layout Group for card content
        var layout = card.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20; // Reduced spacing to maximize space for content
        layout.childControlWidth = true; // Control widths to ensure proper distribution
        layout.childForceExpandWidth = false; // Don't force expand, let flexible handle it
        layout.childControlHeight = true;
        layout.childForceExpandHeight = true;
        layout.padding = new RectOffset(20, 20, 15, 15); // Reduced padding to maximize space for reason
        layout.childAlignment = TextAnchor.MiddleLeft;
        
        // Left section: XP Badge
        CreateXPBadge(card, "+" + xpEvent.delta);
        
        // Middle section: Reason (flexible width)
        CreateReasonSection(card, xpEvent.reason);
        
        // Right section: Date & Time
        CreateDateSection(card, FormatTimestamp(xpEvent.created_at));
        
        Debug.Log($"Card created: {card.name}");
    }
    
    void CreateXPBadge(GameObject parent, string xpValue)
    {
        // Badge container
        GameObject badgeContainer = new GameObject("XPBadge");
        var badgeRect = badgeContainer.AddComponent<RectTransform>();
        badgeContainer.transform.SetParent(parent.transform, false);
        
        // Badge background
        var badgeBg = badgeContainer.AddComponent<UnityEngine.UI.Image>();
        badgeBg.color = new Color(0.95f, 0.85f, 0.3f, 0.9f); // Yellow/gold badge
        
        // Set badge size - compact badge to maximize space for reason
        var badgeLayout = badgeContainer.AddComponent<LayoutElement>();
        badgeLayout.preferredWidth = 100; // Reduced further
        badgeLayout.minWidth = 100;
        badgeLayout.preferredHeight = 70;
        badgeLayout.minHeight = 70;
        badgeLayout.flexibleWidth = 0;
        
        // Badge text
        GameObject badgeText = new GameObject("BadgeText");
        badgeText.transform.SetParent(badgeContainer.transform, false);
        
        var textRect = badgeText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        var tmp = badgeText.AddComponent<TextMeshProUGUI>();
        tmp.text = xpValue;
        tmp.fontSize = 36; // Larger font
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.3f, 0.2f, 0.05f, 1f); // Dark brown/amber text for contrast on yellow
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.enableWordWrapping = false;
        
        // Add shadow
        var shadow = badgeText.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);
        shadow.useGraphicAlpha = true;
    }
    
    void CreateReasonSection(GameObject parent, string reason)
    {
        GameObject reasonContainer = new GameObject("ReasonSection");
        reasonContainer.transform.SetParent(parent.transform, false);
        
        var reasonRect = reasonContainer.AddComponent<RectTransform>();
        reasonRect.anchorMin = new Vector2(0, 0);
        reasonRect.anchorMax = new Vector2(1, 1);
        reasonRect.sizeDelta = Vector2.zero;
        
        var reasonLayout = reasonContainer.AddComponent<LayoutElement>();
        reasonLayout.flexibleWidth = 1; // Takes ALL remaining space
        reasonLayout.minWidth = 600; // Much larger minimum width - maximum space for reason
        reasonLayout.preferredWidth = 0; // Let flexible take over completely
        reasonLayout.preferredHeight = 70;
        reasonLayout.flexibleHeight = 1; // Allow vertical expansion
        
        // Reason text
        var tmp = reasonContainer.AddComponent<TextMeshProUGUI>();
        tmp.text = reason;
        tmp.fontSize = 28; // Larger font for better readability
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.enableWordWrapping = true;
        tmp.wordWrappingRatios = 0.5f; // Better word wrapping
        tmp.lineSpacing = 5; // More line spacing
        
        // Add shadow
        var shadow = reasonContainer.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.75f);
        shadow.effectDistance = new Vector2(2, -2);
        shadow.useGraphicAlpha = true;
    }
    
    void CreateDateSection(GameObject parent, string dateTime)
    {
        GameObject dateContainer = new GameObject("DateSection");
        dateContainer.transform.SetParent(parent.transform, false);
        
        var dateRect = dateContainer.AddComponent<RectTransform>();
        
        var dateLayout = dateContainer.AddComponent<LayoutElement>();
        dateLayout.preferredWidth = 180; // Further reduced to maximize space for reason
        dateLayout.minWidth = 180;
        dateLayout.preferredHeight = 70;
        dateLayout.flexibleWidth = 0;
        
        // Date text
        var tmp = dateContainer.AddComponent<TextMeshProUGUI>();
        tmp.text = dateTime;
        tmp.fontSize = 24; // Larger font
        tmp.color = new Color(0.85f, 0.85f, 0.9f, 1f); // Slightly brighter grey
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.enableWordWrapping = false;
        
        // Add shadow
        var shadow = dateContainer.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.75f);
        shadow.effectDistance = new Vector2(2, -2);
        shadow.useGraphicAlpha = true;
    }
    

    // Deprecated: Old CreateTextCell method - replaced by CreateDataCell with better formatting
    // Keeping for backward compatibility if prefab uses it
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
        shadow.effectColor = new Color(0, 0, 0, 0.75f);
        shadow.effectDistance = new Vector2(3, -3);
        shadow.useGraphicAlpha = true;
        
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
            // Format: MM/dd/yyyy HH:mm (always show full date)
            return dateTime.ToString("MM/dd/yyyy HH:mm");
        }
        return timestamp;
    }

    void LoadCaptainLogData()
    {
        if (fullStudentData == null || captainLogContainer == null)
        {
            Debug.LogWarning("Cannot load captain's log: missing student data or container");
            return;
        }

        // Configure container for full expansion
        var containerRect = captainLogContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
        }

        // Ensure container has VerticalLayoutGroup configured properly
        var verticalLayout = captainLogContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = captainLogContainer.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        }
        
        verticalLayout.childForceExpandWidth = true; // Allow children to expand horizontally (match Stats)
        verticalLayout.childControlWidth = true; // Control child widths (match Stats)
        verticalLayout.childForceExpandHeight = false; // Don't force expand height (match Stats)
        verticalLayout.childControlHeight = false; // Let children control their own height (match Stats)
        verticalLayout.spacing = 150; // Increased spacing between entries for better readability
        verticalLayout.padding = new RectOffset(20, 20, 20, 20); // Match Stats padding

        // Clear existing log entries
        foreach (Transform child in captainLogContainer)
        {
            if (child.gameObject.name != "HeaderRow") // Keep header if it exists
                Destroy(child.gameObject);
        }

        // Get font from actual Stats entries (label or content) to match exactly
        TMPro.TMP_FontAsset statsFont = null;
        Material statsFontMaterial = null;
        
        // Get font from an actual Stats entry that was created
        if (statsPanel != null && statsPanel.transform.childCount > 0)
        {
            // Find the first Stats entry
            var firstStatEntry = statsPanel.transform.GetChild(0);
            
            // Try to get font from label first
            var labelObj = firstStatEntry.Find("Label");
            if (labelObj != null)
            {
                var labelText = labelObj.GetComponent<TextMeshProUGUI>();
                if (labelText != null && labelText.font != null)
                {
                    statsFont = labelText.font;
                    statsFontMaterial = labelText.fontSharedMaterial;
                    Debug.Log($"Captain's Log: Using font '{statsFont.name}' from Stats label");
                }
            }
            
            // If label didn't have font, try content
            if (statsFont == null)
            {
                var contentObj = firstStatEntry.Find("Content");
                if (contentObj != null)
                {
                    var contentText = contentObj.GetComponent<TextMeshProUGUI>();
                    if (contentText != null && contentText.font != null)
                    {
                        statsFont = contentText.font;
                        statsFontMaterial = contentText.fontSharedMaterial;
                        Debug.Log($"Captain's Log: Using font '{statsFont.name}' from Stats content");
                    }
                }
            }
        }
        
        // Fallback to gradeText if Stats entries not found
        if (statsFont == null && gradeText != null && gradeText.font != null)
        {
            statsFont = gradeText.font;
            statsFontMaterial = gradeText.fontSharedMaterial;
            Debug.Log($"Captain's Log: Using fallback font '{statsFont.name}' from gradeText");
        }
        
        if (statsFont == null)
        {
            Debug.LogWarning("Captain's Log: Could not find font from Stats entries or gradeText");
        }

        // Generate log entries from student data
        List<CaptainLogEntry> logEntries = GenerateLogEntries(fullStudentData);

        // Display each log entry with Stats font
        foreach (var entry in logEntries)
        {
            DisplayLogEntry(entry, statsFont, statsFontMaterial);
        }

        Debug.Log($"Displayed {logEntries.Count} captain's log entries");
    }

    List<CaptainLogEntry> GenerateLogEntries(StudentData studentData)
    {
        List<CaptainLogEntry> entries = new List<CaptainLogEntry>();

        // Entry 1: Account creation/journey start
        if (!string.IsNullOrEmpty(studentData.created_at))
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.created_at,
                entry_text = $"Journey start {FormatTimestamp(studentData.created_at)} - {studentData.name} set sail at grade {studentData.grade} to master the art of numbers."
            });
        }

        // Entry 2: Current level achievement
        if (studentData.level > 1)
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.updated_at ?? studentData.created_at,
                entry_text = $"Status: Level {studentData.level} | XP {studentData.xp}/{studentData.xp_goal} | {studentData.xp_goal - studentData.xp} XP to next rank."
            });
        }
        else
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.updated_at ?? studentData.created_at,
                entry_text = $"Status: Level {studentData.level} - just launched with {studentData.xp} XP and clear seas ahead."
            });
        }

        // Entry 3: Time played / dedication
        int hours = studentData.time_played_minutes / 60;
        int minutes = studentData.time_played_minutes % 60;
        if (studentData.time_played_minutes > 0)
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.last_active ?? studentData.updated_at ?? studentData.created_at,
                entry_text = $"Time at sea: {hours}h {minutes}m of focused practice logged."
            });
        }

        // Entry 4: Streak achievement
        if (studentData.streak_days > 0)
        {
            string streakMessage = studentData.streak_days >= 7 
                ? $"Streak {studentData.streak_days} days - momentum strong. " 
                : studentData.streak_days >= 3
                ? $"Streak {studentData.streak_days} days - building consistency. "
                : $"Streak {studentData.streak_days} days - just getting started. ";

            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.last_active ?? studentData.updated_at ?? studentData.created_at,
                entry_text = $"{streakMessage}Keeping the daily rhythm to stay sharp."
            });
        }

        // Entry 5: Behaviour/Performance status
        if (!string.IsNullOrEmpty(studentData.behaviour))
        {
            string behaviourMessage = studentData.behaviour.ToLower().Contains("on-track")
                ? "Performance: on-track and steady."
                : studentData.behaviour.ToLower().Contains("at-risk")
                ? "Performance: at-risk - refocusing efforts."
                : $"Performance status: {studentData.behaviour}.";

            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.updated_at ?? studentData.created_at,
                entry_text = behaviourMessage
            });
        }

        // Entry 6: Current activity
        if (!string.IsNullOrEmpty(studentData.current_activity) && !string.IsNullOrEmpty(studentData.activity_status))
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.last_active ?? studentData.updated_at ?? studentData.created_at,
                entry_text = $"Mission: {studentData.current_activity} | Status: {studentData.activity_status}."
            });
        }

        // Entry 7: Next checkpoint / goal
        if (!string.IsNullOrEmpty(studentData.next_checkpoint))
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.updated_at ?? studentData.created_at,
                entry_text = $"Next checkpoint: {studentData.next_checkpoint}."
            });
        }

        // Entry 8: Last active / recent activity
        if (!string.IsNullOrEmpty(studentData.last_active))
        {
            entries.Add(new CaptainLogEntry
            {
                entry_date = studentData.last_active,
                entry_text = $"Last active: {FormatTimestamp(studentData.last_active)}."
            });
        }

        // Sort entries by date (most recent first)
        entries.Sort((a, b) => 
        {
            DateTime dateA = DateTime.TryParse(a.entry_date, out var dA) ? dA : DateTime.MinValue;
            DateTime dateB = DateTime.TryParse(b.entry_date, out var dB) ? dB : DateTime.MinValue;
            return dateB.CompareTo(dateA); // Descending order
        });

        // Limit to first 5 entries
        if (entries.Count > 5)
        {
            entries = entries.Take(5).ToList();
        }

        return entries;
    }

    void DisplayLogEntry(CaptainLogEntry entry, TMPro.TMP_FontAsset font, Material fontMaterial)
    {
        GameObject logRow;
        
        if (logEntryPrefab != null)
        {
            logRow = Instantiate(logEntryPrefab, captainLogContainer);
            // Clear any existing children if using prefab
            foreach (Transform child in logRow.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            // Create log entry row dynamically
            logRow = new GameObject("LogEntry");
            logRow.AddComponent<RectTransform>();
            logRow.transform.SetParent(captainLogContainer, false);
        }

        // Set up row layout
        var rowRect = logRow.GetComponent<RectTransform>();
        if (rowRect == null)
            rowRect = logRow.AddComponent<RectTransform>();
            
        var rowLayout = logRow.GetComponent<LayoutElement>();
        if (rowLayout == null)
            rowLayout = logRow.AddComponent<LayoutElement>();
            
        rowLayout.minHeight = 150; // Match Stats section
        rowLayout.preferredHeight = 0; // Let content determine height
        rowLayout.flexibleHeight = 1; // Allow rows to expand based on content
        rowLayout.flexibleWidth = 1;
        
        // Ensure row expands to full width
        rowRect.anchorMin = new Vector2(0, 0);
        rowRect.anchorMax = new Vector2(1, 0); // Anchor to top
        rowRect.sizeDelta = Vector2.zero;
        rowRect.pivot = new Vector2(0.5f, 1f); // Pivot at top

        // Add horizontal layout for label and content (match Stats exactly)
        var layout = logRow.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = logRow.AddComponent<HorizontalLayoutGroup>();
            
        layout.spacing = 150; // Match Stats spacing
        layout.padding = new RectOffset(20, 20, 15, 15); // Match Stats padding
        layout.childControlWidth = true; // Match Stats
        layout.childForceExpandWidth = false; // Match Stats
        layout.childControlHeight = false; // Match Stats
        layout.childForceExpandHeight = false; // Match Stats

        // Parse entry text to separate label from content
        string label = "";
        string content = entry.entry_text;
        
        // Extract label (text before colon)
        int colonIndex = entry.entry_text.IndexOf(':');
        if (colonIndex > 0)
        {
            label = entry.entry_text.Substring(0, colonIndex).Trim();
            content = entry.entry_text.Substring(colonIndex + 1).Trim();
        }
        else
        {
            // If no colon, try to extract first word as label
            string[] words = entry.entry_text.Split(new[] { ' ' }, 2);
            if (words.Length > 1)
            {
                label = words[0];
                content = words[1];
            }
            else
            {
                content = entry.entry_text;
            }
        }

        // Create label text (heading)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(logRow.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        
        // Ensure label RectTransform expands within its allocated space
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;
        
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        
        // Explicitly set font to match Stats - same pattern as CreateStatEntry
        if (font != null)
        {
            labelText.font = font;
            labelText.fontSharedMaterial = fontMaterial;
        }
        else if (gradeText != null && gradeText.font != null)
        {
            // Explicit fallback: use Stats font directly
            labelText.font = gradeText.font;
            labelText.fontSharedMaterial = gradeText.fontSharedMaterial;
        }
        
        if (!string.IsNullOrEmpty(label))
        {
            labelText.text = label + ":";
        }
        else
        {
            labelText.text = "";
        }
        
        labelText.color = new Color(1f, 0.9f, 0.6f, 1f); // Golden color for headings
        labelText.fontSize = 52; // Increased font size
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.verticalAlignment = VerticalAlignmentOptions.Top;
        labelText.enableWordWrapping = true; // Allow wrapping if needed
        labelText.overflowMode = TextOverflowModes.Overflow; // Allow overflow instead of truncate
        
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 500; // Increased width to prevent truncation in Captain's Log
        labelLayout.minWidth = 450; // Increased minimum width to prevent cutoff
        labelLayout.flexibleWidth = 0; // Keep label fixed width
        labelLayout.flexibleHeight = 1; // Allow label to expand vertically
        
        // Add shadow to label
        var labelShadow = labelObj.AddComponent<UnityEngine.UI.Shadow>();
        labelShadow.effectColor = new Color(0, 0, 0, 0.75f);
        labelShadow.effectDistance = new Vector2(2, -2);
        labelShadow.useGraphicAlpha = true;

        // Create content text
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(logRow.transform, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        
        // Ensure content RectTransform expands fully
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;
        
        var contentText = contentObj.AddComponent<TextMeshProUGUI>();
        
        // Explicitly set font to match Stats - same pattern as CreateStatEntry
        if (font != null)
        {
            contentText.font = font;
            contentText.fontSharedMaterial = fontMaterial;
        }
        else if (gradeText != null && gradeText.font != null)
        {
            // Explicit fallback: use Stats font directly
            contentText.font = gradeText.font;
            contentText.fontSharedMaterial = gradeText.fontSharedMaterial;
        }
        
        contentText.text = content;
        contentText.color = new Color(0.95f, 0.95f, 0.95f, 1f); // Slightly off-white for readability
        contentText.fontSize = 48; // Increased font size
        contentText.fontStyle = FontStyles.Normal;
        contentText.alignment = TextAlignmentOptions.Left;
        contentText.verticalAlignment = VerticalAlignmentOptions.Top;
        contentText.enableWordWrapping = true; // Allow text to wrap
        contentText.overflowMode = TextOverflowModes.Overflow; // Show full text, don't truncate
        contentText.lineSpacing = 8; // Increased line spacing for larger font
        
        var contentLayout = contentObj.AddComponent<LayoutElement>();
        contentLayout.flexibleWidth = 1; // Takes remaining space (match Stats)
        contentLayout.preferredWidth = 0; // Match Stats
        contentLayout.minWidth = 0; // Match Stats - let flexible width handle it
        contentLayout.flexibleHeight = 1; // Allow content to expand vertically (match Stats)
        
        // Add shadow to content (match Stats)
        var contentShadow = contentObj.AddComponent<UnityEngine.UI.Shadow>();
        contentShadow.effectColor = new Color(0, 0, 0, 0.75f); // Match Stats shadow
        contentShadow.effectDistance = new Vector2(2, -2);
        contentShadow.useGraphicAlpha = true;
    }

}

[Serializable]
public class CaptainLogEntry
{
    public string entry_date;
    public string entry_text;
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
