using System;

[Serializable]
public class Student
{
    public string id;          // UUID from Supabase
    public string name;        // Player name
    public int xp;             // Current XP
    public int xp_goal;        // XP needed for next level
    public int level;          // Current level
    public string behaviour;   // "on-track", "at-risk", etc.
    public string last_active; // ISO date string (optional)
}
