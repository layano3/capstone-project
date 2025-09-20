import { NextResponse } from "next/server";
import { supabaseAdmin } from "@/lib/supabaseAdmin";
import { mapStudentRow } from "@/lib/mapStudent";
import type { StudentUpdate, XpEventInsert } from "@/types/database";

export async function POST(
  request: Request,
  { params }: { params: { id: string } }
) {
  const studentId = params.id;

  if (!studentId) {
    return NextResponse.json({ error: "Student id missing" }, { status: 400 });
  }

  const body = await request.json().catch(() => null);
  if (!body || typeof body.delta !== "number") {
    return NextResponse.json({ error: "Invalid payload" }, { status: 400 });
  }

  const delta = Math.trunc(body.delta);
  const reason = typeof body.reason === "string" && body.reason.trim().length > 0
    ? body.reason.trim().slice(0, 280)
    : "Manual adjustment";
  const updatedBy = typeof body.updatedBy === "string" && body.updatedBy.trim().length > 0
    ? body.updatedBy.trim().slice(0, 120)
    : "Teacher Console";

  const { data: studentRow, error: fetchError } = await supabaseAdmin
    .from("students")
    .select("id, xp")
    .eq("id", studentId)
    .single<{ id: string; xp: number }>();

  if (fetchError || !studentRow) {
    return NextResponse.json({ error: "Student not found" }, { status: 404 });
  }

  const timestamp = new Date().toISOString();
  const newXp = Math.max(0, studentRow.xp + delta);

  const [{ error: updateError }, { error: logError }] = await Promise.all([
    supabaseAdmin
      .from("students")
      // @ts-expect-error -- Supabase type helper mis-infers update payload as never in typed client
      .update<StudentUpdate>({ xp: newXp, updated_at: timestamp })
      .eq("id", studentId),
    supabaseAdmin
      .from("xp_events")
      // @ts-expect-error -- Supabase type helper mis-infers insert payload as never in typed client
      .insert<XpEventInsert>({
        student_id: studentId,
        delta,
        reason,
        updated_by: updatedBy,
        created_at: timestamp,
      }),
  ]);

  if (updateError || logError) {
    console.error("Failed to update XP", updateError ?? logError);
    return NextResponse.json({ error: "Failed to update XP" }, { status: 500 });
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
    return NextResponse.json({ error: "Failed to fetch updated student" }, { status: 500 });
  }

  return NextResponse.json({ student: mapStudentRow(refreshed) });
}
