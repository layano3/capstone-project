using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "MyExposedParam 1"; // Change this to "SFXVolume" if you rename the parameter in the AudioMixer
    private AudioSource musicSource;
    
    [Header("SFX Settings")]
    [SerializeField] private int maxConcurrentSFX = 10;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSFXSources = new List<AudioSource>();

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
            return;
        }
        
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>(); // Try to get the AudioSource if it's not assigned
        }
        
        // Initialize SFX pool
        InitializeSFXPool();
        
        // Try to find SFX mixer group if not assigned
        if (sfxMixerGroup == null && audioMixer != null)
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0)
            {
                sfxMixerGroup = groups[0];
            }
        }
    }
    
    private void InitializeSFXPool()
    {
        for (int i = 0; i < maxConcurrentSFX; i++)
        {
            GameObject sfxObject = new GameObject($"SFXSource_{i}");
            sfxObject.transform.SetParent(transform);
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = sfxMixerGroup;
            sfxPool.Enqueue(source);
        }
    }
    
    private AudioSource GetAvailableSFXSource()
    {
        // Clean up finished sources
        for (int i = activeSFXSources.Count - 1; i >= 0; i--)
        {
            if (!activeSFXSources[i].isPlaying)
            {
                AudioSource source = activeSFXSources[i];
                activeSFXSources.RemoveAt(i);
                sfxPool.Enqueue(source);
            }
        }
        
        // Get from pool or create new if needed
        if (sfxPool.Count > 0)
        {
            return sfxPool.Dequeue();
        }
        
        // If pool is empty, create a temporary source
        GameObject tempObject = new GameObject("TempSFXSource");
        tempObject.transform.SetParent(transform);
        AudioSource tempSource = tempObject.AddComponent<AudioSource>();
        tempSource.playOnAwake = false;
        tempSource.outputAudioMixerGroup = sfxMixerGroup;
        return tempSource;
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
    
    /// <summary>
    /// Play a sound effect at a specific position in 3D space
    /// </summary>
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Attempted to play null AudioClip");
            return;
        }
        
        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend; // 0 = 2D, 1 = 3D
        source.transform.position = position;
        source.Play();
        
        activeSFXSources.Add(source);
        
        // Return to pool when finished
        StartCoroutine(ReturnToPoolWhenFinished(source, clip.length / pitch));
    }
    
    /// <summary>
    /// Play a sound effect at the AudioManager's position (2D)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        PlaySFX(clip, transform.position, volume, pitch, 0f);
    }
    
    /// <summary>
    /// Play a sound effect one-shot style (doesn't require clip assignment)
    /// </summary>
    public void PlaySFXOneShot(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Attempted to play null AudioClip");
            return;
        }
        
        AudioSource source = GetAvailableSFXSource();
        source.transform.position = position;
        source.spatialBlend = 1f; // 3D sound
        source.PlayOneShot(clip, volume);
        
        activeSFXSources.Add(source);
        
        // Return to pool when finished
        StartCoroutine(ReturnToPoolWhenFinished(source, clip.length));
    }
    
    /// <summary>
    /// Play a sound effect one-shot style at AudioManager position (2D)
    /// </summary>
    public void PlaySFXOneShot(AudioClip clip, float volume = 1f)
    {
        PlaySFXOneShot(clip, transform.position, volume);
    }
    
    private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f); // Small buffer
        
        if (source != null && activeSFXSources.Contains(source))
        {
            activeSFXSources.Remove(source);
            if (source.gameObject != null && source.gameObject.name.StartsWith("SFXSource_"))
            {
                sfxPool.Enqueue(source);
            }
            else
            {
                // Temporary source, destroy it
                Destroy(source.gameObject);
            }
        }
    }

}
