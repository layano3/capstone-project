using UnityEngine;

/// <summary>
/// Helper component for easily playing sound effects on game objects.
/// Can be attached to any GameObject to play sounds on events.
/// </summary>
public class SoundEffectHelper : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] soundEffects;
    
    [Header("Settings")]
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitch = 1f;
    [SerializeField] private bool use3DSound = true;
    
    [Header("Local Audio Source (Optional)")]
    [SerializeField] private AudioSource localAudioSource;
    
    private void Start()
    {
        if (localAudioSource == null)
        {
            localAudioSource = GetComponent<AudioSource>();
        }
        
        if (playOnStart && soundEffects.Length > 0)
        {
            PlaySound(0);
        }
    }
    
    /// <summary>
    /// Play a sound effect by index
    /// </summary>
    public void PlaySound(int index)
    {
        if (index < 0 || index >= soundEffects.Length)
        {
            Debug.LogWarning($"SoundEffectHelper: Invalid sound index {index}");
            return;
        }
        
        PlaySound(soundEffects[index]);
    }
    
    /// <summary>
    /// Play a sound effect by AudioClip
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundEffectHelper: Attempted to play null AudioClip");
            return;
        }
        
        if (useAudioManager && AudioManager.Instance != null)
        {
            if (use3DSound)
            {
                AudioManager.Instance.PlaySFXOneShot(clip, transform.position, volume);
            }
            else
            {
                AudioManager.Instance.PlaySFXOneShot(clip, volume);
            }
        }
        else
        {
            // Fallback to local audio source
            if (localAudioSource != null)
            {
                localAudioSource.volume = volume;
                localAudioSource.pitch = pitch;
                localAudioSource.spatialBlend = use3DSound ? 1f : 0f;
                localAudioSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning("SoundEffectHelper: No AudioManager or local AudioSource available");
            }
        }
    }
    
    /// <summary>
    /// Play a random sound effect from the array
    /// </summary>
    public void PlayRandomSound()
    {
        if (soundEffects.Length == 0)
        {
            Debug.LogWarning("SoundEffectHelper: No sound effects configured");
            return;
        }
        
        int randomIndex = Random.Range(0, soundEffects.Length);
        PlaySound(randomIndex);
    }
    
    /// <summary>
    /// Play a sound effect with custom volume and pitch
    /// </summary>
    public void PlaySoundWithSettings(AudioClip clip, float customVolume, float customPitch)
    {
        float originalVolume = volume;
        float originalPitch = pitch;
        
        volume = customVolume;
        pitch = customPitch;
        
        PlaySound(clip);
        
        volume = originalVolume;
        pitch = originalPitch;
    }
}

