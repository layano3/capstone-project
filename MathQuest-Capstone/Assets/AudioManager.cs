using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; 
    public AudioMixer audioMixer; 
    public string musicVolumeParam = "MusicVolume"; 
    public string sfxVolumeParam = "SFXVolume"; 

    private void Awake()
    {
        // Check if there's already an instance of AudioManager
        if (Instance == null)
        {
            // If not, set the current object as the instance and make it persistent
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent the object from being destroyed when loading new scenes
        }
        else
        {
            // If an instance already exists, destroy this one to avoid duplicates
            Destroy(gameObject);
        }
    }

    // Set music volume (0 = mute, 1 = max)
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(musicVolumeParam, Mathf.Log10(volume) * 20); // Log scale for smooth adjustment
    }

    // Set SFX volume (0 = mute, 1 = max)
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(sfxVolumeParam, Mathf.Log10(volume) * 20); // Log scale for smooth adjustment
    }
}
