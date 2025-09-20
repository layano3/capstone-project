export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[];

export interface Database {
  public: {
    Tables: {
      students: {
        Row: {
          id: string;
          name: string;
          grade: string;
          avatar_color: string;
          last_active: string;
          current_activity: string;
          activity_status: "exploring" | "puzzle" | "boss" | "completed" | "offline";
          xp: number;
          xp_goal: number;
          time_played_minutes: number;
          streak_days: number;
          behaviour: "on-track" | "needs-support" | "at-risk";
          next_checkpoint: string | null;
          guardians: string | null;
          created_at: string | null;
          updated_at: string | null;
        };
        Insert: {
          id?: string;
          name: string;
          grade: string;
          avatar_color: string;
          last_active?: string;
          current_activity: string;
          activity_status?: "exploring" | "puzzle" | "boss" | "completed" | "offline";
          xp?: number;
          xp_goal?: number;
          time_played_minutes?: number;
          streak_days?: number;
          behaviour?: "on-track" | "needs-support" | "at-risk";
          next_checkpoint?: string | null;
          guardians?: string | null;
          created_at?: string | null;
          updated_at?: string | null;
        };
        Update: Partial<Database["public"]["Tables"]["students"]["Insert"]>;
      };
      xp_events: {
        Row: {
          id: string;
          student_id: string;
          delta: number;
          reason: string;
          updated_by: string;
          created_at: string;
        };
        Insert: {
          id?: string;
          student_id: string;
          delta: number;
          reason: string;
          updated_by: string;
          created_at?: string;
        };
        Update: Partial<Database["public"]["Tables"]["xp_events"]["Insert"]>;
      };
    };
  };
}
