using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages visual effects (particle systems) throughout the game.
/// Provides pooling and easy spawning of VFX.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    
    [Header("VFX Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private Transform vfxParent;
    
    private Dictionary<GameObject, Queue<GameObject>> vfxPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> activeVFX = new Dictionary<GameObject, GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (vfxParent == null)
            {
                GameObject parent = new GameObject("VFX_Pool");
                parent.transform.SetParent(transform);
                vfxParent = parent.transform;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Spawn a VFX prefab at a position with optional rotation and parent
    /// </summary>
    public GameObject SpawnVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation = default, Transform parent = null, float autoDestroyTime = 0f)
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("VFXManager: Attempted to spawn null VFX prefab");
            return null;
        }
        
        GameObject vfxInstance = GetVFXFromPool(vfxPrefab);
        
        if (vfxInstance == null)
        {
            vfxInstance = Instantiate(vfxPrefab, position, rotation == default ? Quaternion.identity : rotation);
        }
        else
        {
            vfxInstance.transform.position = position;
            vfxInstance.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            vfxInstance.SetActive(true);
        }
        
        if (parent != null)
        {
            vfxInstance.transform.SetParent(parent);
        }
        else
        {
            vfxInstance.transform.SetParent(vfxParent);
        }
        
        // Handle particle systems
        ParticleSystem[] particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            ps.Clear();
            ps.Play();
        }
        
        activeVFX[vfxInstance] = vfxPrefab;
        
        // Auto-destroy if specified
        if (autoDestroyTime > 0f)
        {
            StartCoroutine(AutoDestroyVFX(vfxInstance, autoDestroyTime));
        }
        
        return vfxInstance;
    }
    
    /// <summary>
    /// Spawn a VFX prefab at a position (simplified)
    /// </summary>
    public GameObject SpawnVFX(GameObject vfxPrefab, Vector3 position)
    {
        return SpawnVFX(vfxPrefab, position, Quaternion.identity, null, 0f);
    }
    
    /// <summary>
    /// Spawn a VFX prefab at a transform's position
    /// </summary>
    public GameObject SpawnVFX(GameObject vfxPrefab, Transform target)
    {
        if (target == null) return null;
        return SpawnVFX(vfxPrefab, target.position, target.rotation, null, 0f);
    }
    
    /// <summary>
    /// Return a VFX instance to the pool
    /// </summary>
    public void ReturnVFXToPool(GameObject vfxInstance)
    {
        if (vfxInstance == null || !activeVFX.ContainsKey(vfxInstance))
            return;
        
        GameObject prefab = activeVFX[vfxInstance];
        activeVFX.Remove(vfxInstance);
        
        // Stop and reset particle systems
        ParticleSystem[] particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            ps.Clear();
        }
        
        vfxInstance.SetActive(false);
        vfxInstance.transform.SetParent(vfxParent);
        
        if (!vfxPools.ContainsKey(prefab))
        {
            vfxPools[prefab] = new Queue<GameObject>();
        }
        
        vfxPools[prefab].Enqueue(vfxInstance);
    }
    
    private GameObject GetVFXFromPool(GameObject prefab)
    {
        if (!vfxPools.ContainsKey(prefab))
        {
            vfxPools[prefab] = new Queue<GameObject>();
        }
        
        Queue<GameObject> pool = vfxPools[prefab];
        
        // Check for available instance in pool
        while (pool.Count > 0)
        {
            GameObject instance = pool.Dequeue();
            if (instance != null)
            {
                return instance;
            }
        }
        
        return null;
    }
    
    private System.Collections.IEnumerator AutoDestroyVFX(GameObject vfxInstance, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (vfxInstance != null)
        {
            // Wait for particles to finish if it's a particle system
            ParticleSystem[] particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0f;
            
            foreach (var ps in particles)
            {
                if (ps.main.loop) continue; // Skip looping effects
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (duration > maxDuration)
                    maxDuration = duration;
            }
            
            if (maxDuration > 0f)
            {
                yield return new WaitForSeconds(maxDuration);
            }
            
            ReturnVFXToPool(vfxInstance);
        }
    }
    
    /// <summary>
    /// Pre-warm the pool with instances of a VFX prefab
    /// </summary>
    public void PreWarmPool(GameObject prefab, int count)
    {
        if (prefab == null) return;
        
        if (!vfxPools.ContainsKey(prefab))
        {
            vfxPools[prefab] = new Queue<GameObject>();
        }
        
        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab, vfxParent);
            instance.SetActive(false);
            vfxPools[prefab].Enqueue(instance);
        }
    }
}

