import type { Database } from "@/types/database";
import type { Student } from "@/types/student";

type StudentRow = Database["public"]["Tables"]["students"]["Row"] & {
  xp_events: Database["public"]["Tables"]["xp_events"]["Row"][] | null;
};

export function mapStudentRow(row: StudentRow): Student {
  return {
    id: row.id,
    name: row.name,
    grade: row.grade,
    avatarColor: row.avatar_color,
    lastActive: row.last_active,
    currentActivity: row.current_activity,
    activityStatus: row.activity_status,
    xp: row.xp,
    xpGoal: row.xp_goal,
    timePlayedMinutes: row.time_played_minutes,
    streakDays: row.streak_days,
    behaviour: row.behaviour,
    nextCheckpoint: row.next_checkpoint ?? "",
    guardians: row.guardians ?? undefined,
    xpEvents: (row.xp_events ?? []).map((event) => ({
      id: event.id,
      timestamp: event.created_at,
      delta: event.delta,
      reason: event.reason,
      updatedBy: event.updated_by,
    })),
  };
}
