using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UserAvatar.FinalCharacterController;

/// <summary>
/// Manages the in-game settings panel with sensitivity and SFX volume controls.
/// Settings are saved to PlayerPrefs and applied immediately.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider sensitivityHorizontalSlider;
    [SerializeField] private Slider sensitivityVerticalSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sensitivityHValueText;
    [SerializeField] private TMP_Text sensitivityVValueText;
    [SerializeField] private TMP_Text sfxVolumeValueText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private float defaultSensitivityH = 0.1f;
    [SerializeField] private float defaultSensitivityV = 0.1f;
    [SerializeField] private float defaultSFXVolume = 1f;
    [SerializeField] private float minSensitivity = 0.01f;
    [SerializeField] private float maxSensitivity = 1f;
    [SerializeField] private float sensitivityMultiplier = 10f; // For display purposes (0.1 = 1.0)

    // PlayerPrefs keys
    private const string PREF_SENSITIVITY_H = "MouseSensitivityH";
    private const string PREF_SENSITIVITY_V = "MouseSensitivityV";
    private const string PREF_SFX_VOLUME = "SFXVolume";

    private PlayerController playerController;
    private AudioManager audioManager;

    // Singleton for easy access
    public static SettingsPanel Instance { get; private set; }

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

        // Find audio manager
        audioManager = AudioManager.Instance;

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
            sensitivityHorizontalSlider.onValueChanged.AddListener(OnSensitivityHChanged);
        }

        // Sensitivity Vertical slider
        if (sensitivityVerticalSlider != null)
        {
            sensitivityVerticalSlider.minValue = minSensitivity * sensitivityMultiplier;
            sensitivityVerticalSlider.maxValue = maxSensitivity * sensitivityMultiplier;
            sensitivityVerticalSlider.onValueChanged.AddListener(OnSensitivityVChanged);
        }

        // SFX Volume slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
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
            LoadSettings(); // Refresh displayed values
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
    /// Called when SFX volume slider value changes.
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        // Apply to audio manager
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
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
        HidePanel();

        // If pause menu is open, return to it
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
        {
            // Settings panel is closed, pause menu should remain open
        }
    }

    /// <summary>
    /// Loads settings from PlayerPrefs and applies them.
    /// </summary>
    private void LoadSettings()
    {
        // Load sensitivity values
        float sensitivityH = PlayerPrefs.GetFloat(PREF_SENSITIVITY_H, defaultSensitivityH);
        float sensitivityV = PlayerPrefs.GetFloat(PREF_SENSITIVITY_V, defaultSensitivityV);
        float sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, defaultSFXVolume);

        // Apply to player controller
        if (playerController != null)
        {
            playerController.lookSenseH = sensitivityH;
            playerController.lookSenseV = sensitivityV;
        }

        // Apply to audio manager
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(sfxVolume);
        }

        // Update sliders (multiply by sensitivityMultiplier for display)
        if (sensitivityHorizontalSlider != null)
        {
            sensitivityHorizontalSlider.value = sensitivityH * sensitivityMultiplier;
        }

        if (sensitivityVerticalSlider != null)
        {
            sensitivityVerticalSlider.value = sensitivityV * sensitivityMultiplier;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
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

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = defaultSFXVolume;
        }

        // Save defaults
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_H, defaultSensitivityH);
        PlayerPrefs.SetFloat(PREF_SENSITIVITY_V, defaultSensitivityV);
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

