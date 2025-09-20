export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[];

type StudentActivityStatus = "exploring" | "puzzle" | "boss" | "completed" | "offline";
type StudentBehaviour = "on-track" | "needs-support" | "at-risk";

export interface Database {
  public: {
    Tables: {
      students: {
        Row: StudentRow;
        Insert: StudentInsert;
        Update: StudentUpdate;
      };
      xp_events: {
        Row: XpEventRow;
        Insert: XpEventInsert;
        Update: XpEventUpdate;
      };
    };
    Views: Record<string, never>;
    Functions: Record<string, never>;
    Enums: Record<string, never>;
    CompositeTypes: Record<string, never>;
  };
}

export interface StudentRow {
  id: string;
  name: string;
  grade: string;
  avatar_color: string;
  last_active: string;
  current_activity: string;
  activity_status: StudentActivityStatus;
  xp: number;
  xp_goal: number;
  time_played_minutes: number;
  streak_days: number;
  behaviour: StudentBehaviour;
  next_checkpoint: string | null;
  guardians: string | null;
  created_at: string | null;
  updated_at: string | null;
}

export interface StudentInsert {
  id?: string;
  name: string;
  grade: string;
  avatar_color: string;
  last_active?: string;
  current_activity: string;
  activity_status?: StudentActivityStatus;
  xp?: number;
  xp_goal?: number;
  time_played_minutes?: number;
  streak_days?: number;
  behaviour?: StudentBehaviour;
  next_checkpoint?: string | null;
  guardians?: string | null;
  created_at?: string | null;
  updated_at?: string | null;
}

export type StudentUpdate = Partial<StudentRow>;

export interface XpEventRow {
  id: string;
  student_id: string;
  delta: number;
  reason: string;
  updated_by: string;
  created_at: string;
}

export interface XpEventInsert {
  id?: string;
  student_id: string;
  delta: number;
  reason: string;
  updated_by: string;
  created_at?: string;
}

export type XpEventUpdate = Partial<XpEventRow>;
