using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Supabase;

/// <summary>
/// Manages the overall game session and coordinates tracking systems
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    [Header("References")]
    public SupabaseConfig config;
    public PlaytimeTracker playtimeTracker;
    public StreakManager streakManager;
    public ActivityTracker activityTracker;
    public BehaviourAnalyzer behaviourAnalyzer;
    
    [Header("Auto Setup")]
    public bool createTrackersIfMissing = true;
    
    private static GameSessionManager _instance;
    public static GameSessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSessionManager>();
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
        if (createTrackersIfMissing)
        {
            SetupTrackers();
        }
        
        InitializeSession();
    }
    
    void SetupTrackers()
    {
        // Create PlaytimeTracker if missing
        if (playtimeTracker == null)
        {
            playtimeTracker = gameObject.GetComponent<PlaytimeTracker>();
            if (playtimeTracker == null)
            {
                playtimeTracker = gameObject.AddComponent<PlaytimeTracker>();
            }
        }
        
        // Ensure config is set on playtimeTracker
        if (playtimeTracker != null && playtimeTracker.config == null && config != null)
        {
            playtimeTracker.config = config;
            Debug.Log("GameSessionManager: Set config on PlaytimeTracker");
        }
        else if (playtimeTracker != null && playtimeTracker.config == null)
        {
            Debug.LogError("GameSessionManager: Cannot set config on PlaytimeTracker - config is null!");
        }
        
        // Create StreakManager if missing
        if (streakManager == null)
        {
            streakManager = gameObject.GetComponent<StreakManager>();
            if (streakManager == null)
            {
                streakManager = gameObject.AddComponent<StreakManager>();
                streakManager.config = config;
            }
        }
        
        // Create ActivityTracker if missing
        if (activityTracker == null)
        {
            activityTracker = gameObject.GetComponent<ActivityTracker>();
            if (activityTracker == null)
            {
                activityTracker = gameObject.AddComponent<ActivityTracker>();
                activityTracker.config = config;
            }
        }
        
        // Create BehaviourAnalyzer if missing
        if (behaviourAnalyzer == null)
        {
            behaviourAnalyzer = gameObject.GetComponent<BehaviourAnalyzer>();
            if (behaviourAnalyzer == null)
            {
                behaviourAnalyzer = gameObject.AddComponent<BehaviourAnalyzer>();
                behaviourAnalyzer.config = config;
            }
        }
    }
    
    void InitializeSession()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("No user ID found - session tracking disabled");
            return;
        }
        
        // Update last_active and set initial status based on current scene
        if (activityTracker != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.Contains("Game") || sceneName.Contains("BlackSmith") || sceneName.Contains("Env"))
            {
                activityTracker.StartExploring(sceneName);
            }
            else
            {
                activityTracker.StartExploring(sceneName);
            }
        }
        
        // Check and update streak on login
        // (StreakManager does this automatically in Start)
        
        // Schedule periodic behaviour analysis
        InvokeRepeating(nameof(AnalyzeBehaviour), 300f, 300f); // Every 5 minutes
        
        Debug.Log($"Game session initialized for user: {userId}");
    }
    
    void AnalyzeBehaviour()
    {
        string userId = PlayerPrefs.GetString("CurrentUserId");
        if (string.IsNullOrEmpty(userId) || behaviourAnalyzer == null) return;
        
        StartCoroutine(behaviourAnalyzer.AnalyzeAndUpdateBehaviour(userId));
    }
    
    void OnApplicationQuit()
    {
        // Mark as offline when quitting
        if (activityTracker != null)
        {
            activityTracker.SetOffline();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && activityTracker != null)
        {
            // App pausing, mark as offline
            activityTracker.SetOffline();
        }
    }
}

