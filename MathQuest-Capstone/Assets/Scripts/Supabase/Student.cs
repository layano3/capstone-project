using System;
using Supabase;

namespace Supabase {
    [Serializable]
    public class Student
    {
        public string id;          // UUID from Supabase
        public string name;        // Player name
        public string grade;       // Grade
        public string avatar_color;// Avatar color
        public string last_active; // ISO timestamp
        public string current_activity; // Current activity
        public string activity_status;  // Activity status
        public int xp;             // Current XP
        public int xp_goal;        // XP needed for next level
        public int time_played_minutes; // Time played
        public int streak_days;    // Streak days
        public string behaviour;   // "on-track", "at-risk", etc.
        public string next_checkpoint; // Next checkpoint
        public string guardians;   // Guardians info
        public string created_at;  // Created timestamp
        public string updated_at;  // Updated timestamp
        public int level;          // Current level
    }
}