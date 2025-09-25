using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    // UI sliders for music and SFX volume control
    public Slider musicSlider;
    public Slider sfxSlider;

    public AudioManager audioManager;

    void Start()
    {
        musicSlider.value = 1f;  // Default to max volume (1)
        sfxSlider.value = 1f;    // Default to max volume (1)
    }

    void Update()
    {
        // Update music and SFX volume based on the slider values
        audioManager.SetMusicVolume(musicSlider.value);
        audioManager.SetSFXVolume(sfxSlider.value);
    }
}
