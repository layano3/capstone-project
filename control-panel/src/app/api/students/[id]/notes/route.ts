import { NextResponse } from "next/server";
import { supabaseAdmin } from "@/lib/supabaseAdmin";
import { mapStudentRow } from "@/lib/mapStudent";
import type { XpEventInsert } from "@/types/database";

export async function POST(
  request: Request,
  { params }: { params: { id: string } }
) {
  const studentId = params.id;

  if (!studentId) {
    return NextResponse.json({ error: "Student id missing" }, { status: 400 });
  }

  const body = await request.json().catch(() => null);
  const note = typeof body?.note === "string" ? body.note.trim() : "";
  const updatedBy =
    typeof body?.updatedBy === "string" && body.updatedBy.trim().length > 0
      ? body.updatedBy.trim().slice(0, 120)
      : "Teacher Console";

  if (!note) {
    return NextResponse.json({ error: "Note is required" }, { status: 400 });
  }

  const { data: studentRow, error: fetchError } = await supabaseAdmin
    .from("students")
    .select("id")
    .eq("id", studentId)
    .single<{ id: string }>();

  if (fetchError || !studentRow) {
    return NextResponse.json({ error: "Student not found" }, { status: 404 });
  }

  const timestamp = new Date().toISOString();

  const { error: logError } = await supabaseAdmin
    .from("xp_events")
    // @ts-expect-error -- Supabase type helper mis-infers insert payload as never in typed client
    .insert<XpEventInsert>({
      student_id: studentId,
      delta: 0,
      reason: note.slice(0, 280),
      updated_by: updatedBy,
      created_at: timestamp,
    });

  if (logError) {
    console.error("Failed to add note", logError);
    return NextResponse.json({ error: "Failed to add note" }, { status: 500 });
  }

  const { data: refreshed, error: refreshError } = await supabaseAdmin
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
    .eq("id", studentId)
    .order("created_at", { ascending: false, foreignTable: "xp_events" })
    .single();

  if (refreshError || !refreshed) {
    console.error("Failed to fetch updated student", refreshError);
    return NextResponse.json({ error: "Failed to fetch updated student" }, { status: 500 });
  }

  return NextResponse.json({ student: mapStudentRow(refreshed) });
}
