using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the Sound and VFX systems.
/// This is a reference guide - you can delete this file if not needed.
/// </summary>
public class Example_SoundAndVFXUsage : MonoBehaviour
{
    [Header("Example Audio Clips")]
    [SerializeField] private AudioClip exampleSound;
    
    [Header("Example VFX Prefabs")]
    [SerializeField] private GameObject exampleVFX;
    
    void Update()
    {
        // Example 1: Play sound effect using AudioManager
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (AudioManager.Instance != null && exampleSound != null)
            {
                // Play 3D sound at a position
                AudioManager.Instance.PlaySFXOneShot(exampleSound, transform.position, 1f);
                
                // Or play 2D sound (no position)
                // AudioManager.Instance.PlaySFXOneShot(exampleSound, 1f);
            }
        }
        
        // Example 2: Spawn VFX using VFXManager
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (VFXManager.Instance != null && exampleVFX != null)
            {
                // Spawn VFX at position (auto-destroy after 3 seconds)
                VFXManager.Instance.SpawnVFX(exampleVFX, transform.position, Quaternion.identity, null, 3f);
                
                // Or spawn at a transform
                // VFXManager.Instance.SpawnVFX(exampleVFX, transform);
            }
        }
        
        // Example 3: Using SoundEffectHelper component
        // Just attach SoundEffectHelper to any GameObject and assign sounds in the inspector
        // Then call: GetComponent<SoundEffectHelper>().PlaySound(0);
        
        // Example 4: Using VFXHelper component
        // Just attach VFXHelper to any GameObject and assign VFX prefabs in the inspector
        // Then call: GetComponent<VFXHelper>().SpawnVFX(0);
    }
}

