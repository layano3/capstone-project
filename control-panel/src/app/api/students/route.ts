import { NextResponse } from "next/server";
import { supabaseAdmin } from "@/lib/supabaseAdmin";
import { mapStudentRow } from "@/lib/mapStudent";

export async function GET() {
  const { data, error } = await supabaseAdmin
    .from("students")
    .select(
      `
        id,
        name,
        grade,
        avatar_color,
        last_active,
        current_activity,
        activity_status,
        xp,
        xp_goal,
        time_played_minutes,
        streak_days,
        behaviour,
        next_checkpoint,
        guardians,
        xp_events (
          id,
          delta,
          reason,
          updated_by,
          created_at
        )
      `
    )
    .order("last_active", { ascending: false })
    .order("created_at", { ascending: false, foreignTable: "xp_events" });

  if (error) {
    console.error("Failed to fetch students", error);
    return NextResponse.json({ error: error.message }, { status: 500 });
  }

  const students = (data ?? []).map(mapStudentRow);
  return NextResponse.json({ students });
}
