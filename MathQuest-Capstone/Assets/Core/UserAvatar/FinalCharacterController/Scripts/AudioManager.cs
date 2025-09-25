using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";
    private AudioSource musicSource;

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
        
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>(); // Try to get the AudioSource if it's not assigned
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        else
        {
            Debug.LogWarning("MusicSource is either null or not playing.");
        }
    }

    public void SetMusicVolume(float volume)
    {
        // Prevent log of zero by setting the volume to a small value if it is zero
        float dbValue = volume == 0 ? -80f : Mathf.Log10(volume) * 20; // -80dB is typically silence
        audioMixer.SetFloat(musicVolumeParam, dbValue);
    }

    public void SetSFXVolume(float volume)
    {
        // Prevent log of zero by setting the volume to a small value if it is zero
        float dbValue = volume == 0 ? -80f : Mathf.Log10(volume) * 20; // -80dB is typically silence
        audioMixer.SetFloat(sfxVolumeParam, dbValue);
    }

}
