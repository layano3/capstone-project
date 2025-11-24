using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the pause menu system. Shows/hides pause menu on Escape key press.
/// Provides options for: Return to Main Menu, Settings, and Unstuck.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button unstuckButton;
    [SerializeField] private Button resumeButton;

    [Header("Menu Containers")]
    [Tooltip("The container with pause menu buttons (Main Menu, Settings, Unstuck, Resume)")]
    [SerializeField] private GameObject menuContainer;
    [Tooltip("The settings panel container (nested inside pause menu panel)")]
    [SerializeField] private GameObject settingsContainer;

    [Header("Settings Panel")]
    [SerializeField] private SettingsPanel settingsPanel;

    [Header("Settings")]
    [SerializeField] private bool pauseTimeOnOpen = true;
    [SerializeField] private bool lockCursorOnResume = true;

    private bool isPaused = false;
    private CursorLockMode previousCursorState;
    private bool previousCursorVisible;

    // Singleton pattern for easy access
    public static PauseMenuManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize pause menu as hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (unstuckButton != null)
        {
            unstuckButton.onClick.AddListener(OnUnstuckClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Auto-find settings panel if not assigned
        if (settingsPanel == null)
        {
            settingsPanel = FindObjectOfType<SettingsPanel>();
        }
        
        // Auto-find containers if not assigned
        if (menuContainer == null && pauseMenuPanel != null)
        {
            // Try to find MenuContainer as a child of pauseMenuPanel
            Transform menuContainerTransform = pauseMenuPanel.transform.Find("MenuContainer");
            if (menuContainerTransform != null)
            {
                menuContainer = menuContainerTransform.gameObject;
            }
        }
        
        if (settingsContainer == null)
        {
            // Try to find SettingsPanel GameObject (the container, not the component)
            if (settingsPanel != null)
            {
                settingsContainer = settingsPanel.gameObject;
            }
            else if (pauseMenuPanel != null)
            {
                Transform settingsTransform = pauseMenuPanel.transform.Find("SettingsPanel");
                if (settingsTransform != null)
                {
                    settingsContainer = settingsTransform.gameObject;
                }
            }
        }
        
        // Ensure initial state: menu container visible, settings container hidden
        if (menuContainer != null)
        {
            menuContainer.SetActive(true);
        }
        if (settingsContainer != null)
        {
            settingsContainer.SetActive(false);
        }
    }

    private void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings container is visible, close it and return to menu
            if (settingsContainer != null && settingsContainer.activeSelf)
            {
                ShowPauseMenu(); // This will hide settings and show menu
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                // Don't pause if a puzzle is active
                if (PuzzleHelper.IsAnyPuzzleActive())
                {
                    return;
                }

                PauseGame();
            }
        }
    }

    /// <summary>
    /// Pauses the game and shows the pause menu.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Save current cursor state
        previousCursorState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Pause game time if enabled
        if (pauseTimeOnOpen)
        {
            Time.timeScale = 0f;
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player input (optional - depends on your input system)
        DisablePlayerInput();
    }

    /// <summary>
    /// Shows the pause menu panel (used when returning from settings).
    /// </summary>
    public void ShowPauseMenu()
    {
        if (!isPaused) 
        {
            Debug.LogWarning("PauseMenuManager: Cannot show pause menu - game is not paused.");
            return; // Only show if game is paused
        }

        // Hide settings container and show menu container
        if (settingsContainer != null)
        {
            settingsContainer.SetActive(false);
        }
        
        if (menuContainer != null)
        {
            menuContainer.SetActive(true);
            Debug.Log("PauseMenuManager: Menu container shown.");
        }
        else
        {
            Debug.LogError("PauseMenuManager: menuContainer is null. Cannot show pause menu.");
        }
    }

    /// <summary>
    /// Resumes the game and hides the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Resume game time
        Time.timeScale = 1f;

        // Restore cursor state
        if (lockCursorOnResume)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = previousCursorState;
            Cursor.visible = previousCursorVisible;
        }

        // Re-enable player input
        EnablePlayerInput();
    }

    /// <summary>
    /// Called when Main Menu button is clicked.
    /// </summary>
    private void OnMainMenuClicked()
    {
        // Resume time before scene change
        Time.timeScale = 1f;

        // Clean up QuestManager if it exists
        if (QuestManager.Instance != null)
        {
            Destroy(QuestManager.Instance.gameObject);
        }

        // Load main menu scene (scene index 0)
        SceneManager.LoadSceneAsync(0);
    }

    /// <summary>
    /// Called when Settings button is clicked. Shows the in-game settings panel.
    /// </summary>
    private void OnSettingsClicked()
    {
        ShowSettings();
    }
    
    /// <summary>
    /// Public method to show settings - can be called from Unity Events.
    /// </summary>
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            // Hide menu container and show settings container
            if (menuContainer != null)
            {
                menuContainer.SetActive(false);
                Debug.Log("PauseMenuManager: Menu container hidden.");
            }
            
            if (settingsContainer != null)
            {
                settingsContainer.SetActive(true);
                Debug.Log("PauseMenuManager: Settings container shown.");
            }
            else
            {
                Debug.LogWarning("PauseMenuManager: settingsContainer is null. Trying to use settingsPanel GameObject.");
                if (settingsPanel != null && settingsPanel.GetSettingsContainer() != null)
                {
                    settingsPanel.GetSettingsContainer().SetActive(true);
                }
            }
            
            settingsPanel.ShowPanel();
        }
        else
        {
            // Fallback: Load settings scene if panel not available
            Debug.LogWarning("PauseMenuManager: SettingsPanel not found. Loading settings scene instead.");
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync(4);
        }
    }

    /// <summary>
    /// Called when Unstuck button is clicked. Resets player to initial spawn position.
    /// </summary>
    private void OnUnstuckClicked()
    {
        // Find PlayerSpawnTracker and reset player position
        PlayerSpawnTracker spawnTracker = FindObjectOfType<PlayerSpawnTracker>();
        
        if (spawnTracker != null)
        {
            spawnTracker.ResetPlayerToSpawn();
            ResumeGame(); // Close the pause menu after unstuck
        }
        else
        {
            Debug.LogWarning("PauseMenuManager: PlayerSpawnTracker not found in scene. Cannot reset player position.");
        }
    }

    /// <summary>
    /// Disables player input systems when paused.
    /// </summary>
    private void DisablePlayerInput()
    {
        // Disable player locomotion input
        var playerLocomotion = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerLocomotionInput>();
        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = false;
        }

        // Disable player interact input
        var playerInteract = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerInteractInput>();
        if (playerInteract != null)
        {
            playerInteract.enabled = false;
        }
    }

    /// <summary>
    /// Re-enables player input systems when resuming.
    /// </summary>
    private void EnablePlayerInput()
    {
        // Re-enable player locomotion input
        var playerLocomotion = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerLocomotionInput>();
        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = true;
        }

        // Re-enable player interact input
        var playerInteract = FindObjectOfType<UserAvatar.FinalCharacterController.PlayerInteractInput>();
        if (playerInteract != null)
        {
            playerInteract.enabled = true;
        }
    }

    /// <summary>
    /// Public property to check if game is paused.
    /// </summary>
    public bool IsPaused => isPaused;

    private void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}

