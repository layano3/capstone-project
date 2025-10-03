using UnityEngine;

[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "Config/Supabase")]
public class SupabaseConfig : ScriptableObject
{
    [Header("From Supabase Settings → API")]
    public string url;    // e.g. https://xxxx.supabase.co
    public string anonKey;

    [Header("Default paths (usually no change)")]
    public string authPath = "/auth/v1";
    public string restPath = "/rest/v1";
}
