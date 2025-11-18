using UnityEngine;

/// <summary>
/// Helper component for easily spawning visual effects on game objects.
/// Can be attached to any GameObject to spawn VFX on events.
/// </summary>
public class VFXHelper : MonoBehaviour
{
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject[] vfxPrefabs;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private bool spawnAtPosition = true;
    [SerializeField] private bool spawnAtRotation = true;
    [SerializeField] private bool parentToThis = false;
    [SerializeField] private float autoDestroyTime = 0f; // 0 = don't auto-destroy
    
    [Header("Offset")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    
    private void Start()
    {
        if (spawnOnStart && vfxPrefabs.Length > 0)
        {
            SpawnVFX(0);
        }
    }
    
    /// <summary>
    /// Spawn a VFX by index
    /// </summary>
    public GameObject SpawnVFX(int index)
    {
        if (index < 0 || index >= vfxPrefabs.Length)
        {
            Debug.LogWarning($"VFXHelper: Invalid VFX index {index}");
            return null;
        }
        
        return SpawnVFX(vfxPrefabs[index]);
    }
    
    /// <summary>
    /// Spawn a VFX prefab
    /// </summary>
    public GameObject SpawnVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("VFXHelper: Attempted to spawn null VFX prefab");
            return null;
        }
        
        Vector3 spawnPosition = spawnAtPosition ? transform.position + positionOffset : transform.position + positionOffset;
        Quaternion spawnRotation = spawnAtRotation ? transform.rotation * Quaternion.Euler(rotationOffset) : Quaternion.Euler(rotationOffset);
        Transform parent = parentToThis ? transform : null;
        
        if (VFXManager.Instance != null)
        {
            return VFXManager.Instance.SpawnVFX(vfxPrefab, spawnPosition, spawnRotation, parent, autoDestroyTime);
        }
        else
        {
            // Fallback: instantiate directly
            GameObject instance = Instantiate(vfxPrefab, spawnPosition, spawnRotation);
            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }
            
            // Auto-destroy if specified
            if (autoDestroyTime > 0f)
            {
                Destroy(instance, autoDestroyTime);
            }
            
            return instance;
        }
    }
    
    /// <summary>
    /// Spawn a random VFX from the array
    /// </summary>
    public GameObject SpawnRandomVFX()
    {
        if (vfxPrefabs.Length == 0)
        {
            Debug.LogWarning("VFXHelper: No VFX prefabs configured");
            return null;
        }
        
        int randomIndex = Random.Range(0, vfxPrefabs.Length);
        return SpawnVFX(randomIndex);
    }
    
    /// <summary>
    /// Spawn a VFX at a specific position
    /// </summary>
    public GameObject SpawnVFXAtPosition(GameObject vfxPrefab, Vector3 position)
    {
        if (vfxPrefab == null) return null;
        
        if (VFXManager.Instance != null)
        {
            return VFXManager.Instance.SpawnVFX(vfxPrefab, position, Quaternion.identity, null, autoDestroyTime);
        }
        else
        {
            GameObject instance = Instantiate(vfxPrefab, position, Quaternion.identity);
            if (autoDestroyTime > 0f)
            {
                Destroy(instance, autoDestroyTime);
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Stop all VFX spawned by this helper
    /// </summary>
    public void StopAllVFX()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
        }
    }
}

