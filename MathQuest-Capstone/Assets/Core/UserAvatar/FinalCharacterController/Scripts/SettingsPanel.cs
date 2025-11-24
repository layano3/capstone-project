using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UserAvatar.FinalCharacterController;

/// <summary>
/// Manages the in-game settings panel with sensitivity, music volume, and SFX volume controls.
/// Settings are saved to PlayerPrefs and applied immediately.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider sensitivityHorizontalSlider;
    [SerializeField] private Slider sensitivityVerticalSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sensitivityHValueText;
    [SerializeField] private TMP_Text sensitivityVValueText;
    [SerializeField] private TMP_Text musicVolumeValueText;
    [SerializeField] private TMP_Text sfxVolumeValueText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private float defaultSensitivityH = 0.1f;
    [SerializeField] private float defaultSensitivityV = 0.1f;
    [SerializeField] private float defaultMusicVolume = 1f;
    [SerializeField] private float defaultSFXVolume = 1f;
    [SerializeField] private float minSensitivity = 0.01f;
    [SerializeField] private float maxSensitivity = 1f;
    [SerializeField] private float sensitivityMultiplier = 10f; // For display purposes (0.1 = 1.0)

    // PlayerPrefs keys
    private const string PREF_SENSITIVITY_H = "MouseSensitivityH";
    private const string PREF_SENSITIVITY_V = "MouseSensitivityV";
    private const string PREF_MUSIC_VOLUME = "MusicVolume";
    private const string PREF_SFX_VOLUME = "SFXVolume";

    private PlayerController playerController;
    private AudioManager audioManager;

    // Singleton for easy access
    public static SettingsPanel Instance { get; private set; }

    /// <summary>
    /// Gets the AudioManager instance. Since AudioManager persists from main menu,
    /// this should always return the same instance.
    /// </summary>
    private AudioManager GetAudioManager()
    {
        // Try the cached reference first
        if (audioManager != null)
        {
            return audioManager;
        }

        // Try the singleton instance (persistent from main menu)
        if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
            return audioManager;
        }

        // Fallback: search for it in the scene (shouldn't be needed if it persists)
        audioManager = FindObjectOfType<AudioManager>();
        return audioManager;
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize panel as hidden
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();

        // Get the persistent AudioManager instance (created in main menu)
        audioManager = GetAudioManager();
        if (audioManager == null)
        {
            Debug.LogWarning("SettingsPanel: AudioManager not found. Music and SFX volume controls will not work. Make sure AudioManager exists in the main menu scene and persists across scenes.");
        }

        // Setup sliders
        SetupSliders();

        // Load saved settings
        LoadSettings();

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void SetupSliders()
    {
        // Sensitivity Horizontal slider
        if (sensitivityHorizontalSlider != null)
        {
            sensitivityHorizontalSlider.minValue = minSensitivity * sensitivityMultiplier;
            sensitivityHorizontalSlider.maxValue = maxSensitivity * sensitivityMultiplier;
            // Remove existing listeners to avoid duplicates
            sensitivityHorizontalSlider.onValueChanged.RemoveAllListeners();
            sensitivityHorizontalSlider.onValueChanged.AddListener(OnSensitivityHChanged);
        }

        // Sensitivity Vertical slider
        if (sensitivityVerticalSlider != null)
        {
            sensitivityVerticalSlider.minValue = minSensitivity * sensitivityMultiplier;
            sensitivityVerticalSlider.maxValue = maxSensitivity * sensitivityMultiplier;
            // Remove existing listeners to avoid duplicates
            sensitivityVerticalSlider.onValueChanged.RemoveAllListeners();
            sensitivityVerticalSlider.onValueChanged.AddListener(OnSensitivityVChanged);
        }

        // Music Volume slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            // Remove existing listeners to avoid duplicates
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // SFX Volume slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            // Remove existing listeners to avoid duplicates
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    /// <summary>
    /// Shows the settings panel.
    /// </summary>
    public void ShowPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            RefreshDisplayedValues(); // Refresh displayed values without triggering listeners
        }
    }

    /// <summary>
    /// Hides the settings panel.
    /// </summary>
    public void HidePanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Gets the settings panel GameObject (the container).
    /// </summary>
    public GameObject GetSettingsContainer()
    {
        return settingsPanel;
    }

    /// <summary>
    /// Toggles the settings panel visibility.
    /// </summary>
    public void TogglePanel()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    /// <summary>
    /// Called when horizontal sensitivity slider value changes.
    /// </summary>
    private void OnSensitivityHChanged(float value)
    {
        // Convert slider value back to actual sensitivity (divide by multiplier)
        float actualSensitivity = value / sensitivityMultiplier;

        // Apply to player controller
        if (playerController != null)
        {
            playerController.lookSenseH = actualSensitivity;
        }

        // Update display text
        if (sensitivityHValueText != null)
        {
            sensitivityHValueText.text = value.ToString("F1");
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_H, actualSensitivity);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called when vertical sensitivity slider value changes.
    /// </summary>
    private void OnSensitivityVChanged(float value)
    {
        // Convert slider value back to actual sensitivity (divide by multiplier)
        float actualSensitivity = value / sensitivityMultiplier;

        // Apply to player controller
        if (playerController != null)
        {
            playerController.lookSenseV = actualSensitivity;
        }

        // Update display text
        if (sensitivityVValueText != null)
        {
            sensitivityVValueText.text = value.ToString("F1");
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_V, actualSensitivity);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called when music volume slider value changes.
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        // Get the persistent AudioManager instance
        AudioManager manager = GetAudioManager();
        
        if (manager != null)
        {
            manager.SetMusicVolume(value);
        }
        else
        {
            Debug.LogWarning("SettingsPanel: AudioManager not found. Cannot set music volume. Make sure AudioManager exists in the main menu scene.");
        }

        // Update display text (show as percentage)
        if (musicVolumeValueText != null)
        {
            musicVolumeValueText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called when SFX volume slider value changes.
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        // Get the persistent AudioManager instance
        AudioManager manager = GetAudioManager();
        
        if (manager != null)
        {
            manager.SetSFXVolume(value);
        }
        else
        {
            Debug.LogWarning("SettingsPanel: AudioManager not found. Cannot set SFX volume. Make sure AudioManager exists in the main menu scene.");
        }

        // Update display text (show as percentage)
        if (sfxVolumeValueText != null)
        {
            sfxVolumeValueText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called when close button is clicked.
    /// </summary>
    private void OnCloseClicked()
    {
        // Hide settings panel
        HidePanel();

        // Show pause menu again if game is still paused
        if (PauseMenuManager.Instance != null)
        {
            if (PauseMenuManager.Instance.IsPaused)
            {
                PauseMenuManager.Instance.ShowPauseMenu();
            }
        }
        else
        {
            Debug.LogWarning("SettingsPanel: PauseMenuManager.Instance is null. Cannot return to pause menu.");
        }
    }
    
    /// <summary>
    /// Public method to handle close button - can be called from Unity Events.
    /// </summary>
    public void OnCloseButtonClicked()
    {
        OnCloseClicked();
    }

    /// <summary>
    /// Loads settings from PlayerPrefs and applies them (called on Start).
    /// </summary>
    private void LoadSettings()
    {
        // Load sensitivity values
        float sensitivityH = PlayerPrefs.GetFloat(PREF_SENSITIVITY_H, defaultSensitivityH);
        float sensitivityV = PlayerPrefs.GetFloat(PREF_SENSITIVITY_V, defaultSensitivityV);
        float musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, defaultSFXVolume);

        // Apply to player controller
        if (playerController != null)
        {
            playerController.lookSenseH = sensitivityH;
            playerController.lookSenseV = sensitivityV;
        }

        // Apply to audio manager using the persistent instance
        AudioManager manager = GetAudioManager();
        if (manager != null)
        {
            manager.SetMusicVolume(musicVolume);
            manager.SetSFXVolume(sfxVolume);
        }

        // Update sliders (multiply by sensitivityMultiplier for display)
        // Temporarily remove listeners to avoid triggering callbacks
        RefreshDisplayedValues();
    }

    /// <summary>
    /// Refreshes displayed values without triggering change listeners.
    /// </summary>
    private void RefreshDisplayedValues()
    {
        // Load current values from PlayerPrefs
        float sensitivityH = PlayerPrefs.GetFloat(PREF_SENSITIVITY_H, defaultSensitivityH);
        float sensitivityV = PlayerPrefs.GetFloat(PREF_SENSITIVITY_V, defaultSensitivityV);
        float musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, defaultSFXVolume);

        // Temporarily remove listeners
        if (sensitivityHorizontalSlider != null)
        {
            sensitivityHorizontalSlider.onValueChanged.RemoveListener(OnSensitivityHChanged);
            sensitivityHorizontalSlider.value = sensitivityH * sensitivityMultiplier;
            sensitivityHorizontalSlider.onValueChanged.AddListener(OnSensitivityHChanged);
        }

        if (sensitivityVerticalSlider != null)
        {
            sensitivityVerticalSlider.onValueChanged.RemoveListener(OnSensitivityVChanged);
            sensitivityVerticalSlider.value = sensitivityV * sensitivityMultiplier;
            sensitivityVerticalSlider.onValueChanged.AddListener(OnSensitivityVChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Update display texts
        UpdateDisplayTexts();
    }

    /// <summary>
    /// Updates all display text values.
    /// </summary>
    private void UpdateDisplayTexts()
    {
        if (sensitivityHValueText != null && sensitivityHorizontalSlider != null)
        {
            sensitivityHValueText.text = sensitivityHorizontalSlider.value.ToString("F1");
        }

        if (sensitivityVValueText != null && sensitivityVerticalSlider != null)
        {
            sensitivityVValueText.text = sensitivityVerticalSlider.value.ToString("F1");
        }

        if (musicVolumeValueText != null && musicVolumeSlider != null)
        {
            musicVolumeValueText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100f).ToString() + "%";
        }

        if (sfxVolumeValueText != null && sfxVolumeSlider != null)
        {
            sfxVolumeValueText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100f).ToString() + "%";
        }
    }

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    public void ResetToDefaults()
    {
        // Reset sliders to defaults
        if (sensitivityHorizontalSlider != null)
        {
            sensitivityHorizontalSlider.value = defaultSensitivityH * sensitivityMultiplier;
        }

        if (sensitivityVerticalSlider != null)
        {
            sensitivityVerticalSlider.value = defaultSensitivityV * sensitivityMultiplier;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = defaultMusicVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = defaultSFXVolume;
        }

        // Save defaults
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_H, defaultSensitivityH);
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_V, defaultSensitivityV);
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, defaultMusicVolume);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, defaultSFXVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Checks if the settings panel is currently visible.
    /// </summary>
    public bool IsVisible()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }
}

