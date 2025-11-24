"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { ClipboardList, FileSpreadsheet, Send } from "lucide-react";
import type { Student } from "@/types/student";

interface QuickActionsProps {
  students: Student[];
  selectedId: string | null;
  onAddNote: (studentId: string, note: string) => Promise<void>;
  savingIds: Set<string>;
}

export function QuickActions({ students, selectedId, onAddNote, savingIds }: QuickActionsProps) {
  const [studentId, setStudentId] = useState<string>("");
  const [note, setNote] = useState<string>("");
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const noteRef = useRef<HTMLTextAreaElement | null>(null);

  useEffect(() => {
    if (students.length === 0) {
      setStudentId("");
      return;
    }

    if (selectedId && students.some((student) => student.id === selectedId)) {
      setStudentId(selectedId);
      return;
    }

    if (!studentId || !students.some((student) => student.id === studentId)) {
      setStudentId(students[0].id);
    }
  }, [selectedId, students, studentId]);

  const isSaving = useMemo(
    () => (studentId ? savingIds.has(studentId) : false),
    [savingIds, studentId]
  );

  const handleExport = () => {
    if (students.length === 0) return;

    const headers = [
      "Student",
      "Grade",
      "Activity",
      "Status",
      "XP",
      "XP Goal",
      "Progress %",
      "Behaviour",
      "Time Played (min)",
      "Streak Days",
      "Next Checkpoint",
      "Last Active",
    ];

    const escape = (value: string | number) =>
      `"${String(value ?? "").replace(/"/g, '""')}"`;

    const rows = students.map((student) => {
      const progress =
        student.xpGoal > 0
          ? Math.min(100, Math.round((student.xp / student.xpGoal) * 100))
          : 0;
      return [
        student.name,
        student.grade,
        student.currentActivity,
        student.activityStatus,
        student.xp,
        student.xpGoal,
        `${progress}%`,
        student.behaviour,
        student.timePlayedMinutes,
        student.streakDays,
        student.nextCheckpoint,
        student.lastActive,
      ];
    });

    const csv = [headers, ...rows]
      .map((row) => row.map(escape).join(","))
      .join("\n");

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `mathquest-roster-${new Date().toISOString().slice(0, 10)}.csv`;
    link.style.display = "none";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const handleSubmit = async () => {
    if (!studentId) {
      setError("Select a student first");
      return;
    }

    const trimmed = note.trim();
    if (!trimmed) {
      setError("Add a quick note before logging");
      return;
    }

    setSubmitting(true);
    setError(null);
    setMessage(null);

    try {
      await onAddNote(studentId, trimmed);
      setNote("");
      setMessage("Note added to the behaviour log.");
      noteRef.current?.focus();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to add note");
    } finally {
      setSubmitting(false);
    }
  };

  const focusNoteField = () => {
    noteRef.current?.focus();
  };

  return (
    <section className="card p-6">
      <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Quick actions</p>
      <ul className="mt-4 space-y-3">
        <li>
          <button
            type="button"
            onClick={handleExport}
            disabled={students.length === 0}
            className="flex w-full items-center gap-3 rounded-2xl border border-white/5 bg-white/5 px-4 py-3 text-left transition hover:border-primary-400/40 hover:bg-primary-500/10 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-500/15 text-primary-200">
              <FileSpreadsheet className="h-5 w-5" />
            </span>
            <div>
              <p className="text-sm font-semibold text-slate-100">Export snapshot</p>
              <p className="text-xs text-slate-500">Download CSV of current XP and progress</p>
            </div>
          </button>
        </li>
        <li>
          <button
            type="button"
            onClick={focusNoteField}
            className="flex w-full items-center gap-3 rounded-2xl border border-white/5 bg-white/5 px-4 py-3 text-left transition hover:border-primary-400/40 hover:bg-primary-500/10"
          >
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-500/15 text-primary-200">
              <ClipboardList className="h-5 w-5" />
            </span>
            <div>
              <p className="text-sm font-semibold text-slate-100">Log behaviour note</p>
              <p className="text-xs text-slate-500">Quick notes post to the behaviour log</p>
            </div>
          </button>
        </li>
        <li>
          <button
            type="button"
            className="flex w-full items-center gap-3 rounded-2xl border border-white/5 bg-white/5 px-4 py-3 text-left transition hover:border-primary-400/40 hover:bg-primary-500/10"
          >
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-500/15 text-primary-200">
              <Send className="h-5 w-5" />
            </span>
            <div>
              <p className="text-sm font-semibold text-slate-100">Message guardians</p>
              <p className="text-xs text-slate-500">Send weekly digest to selected families</p>
            </div>
          </button>
        </li>
      </ul>

      <div id="quick-note" className="mt-5 rounded-2xl border border-white/5 bg-slate-900/60 p-4">
        <div className="flex items-center justify-between gap-3">
          <div>
            <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Quick note</p>
            <p className="text-sm text-slate-200">Post directly into behaviour notes.</p>
          </div>
          <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-400">
            {students.length} students
          </span>
        </div>
        <div className="mt-4 space-y-3">
          <label className="block text-xs font-semibold uppercase tracking-[0.3em] text-primary-200">
            Student
            <select
              value={studentId}
              onChange={(event) => setStudentId(event.target.value)}
              className="mt-2 w-full rounded-xl border border-primary-500/40 bg-slate-950/80 px-4 py-2 text-sm text-slate-100 focus:border-primary-400 focus:outline-none focus:ring-2 focus:ring-primary-400/40"
              disabled={submitting || students.length === 0}
            >
              {students.map((student) => (
                <option key={student.id} value={student.id}>
                  {student.name} · {student.grade}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-xs font-semibold uppercase tracking-[0.3em] text-primary-200">
            Note
            <textarea
              ref={noteRef}
              value={note}
              onChange={(event) => setNote(event.target.value)}
              rows={3}
              placeholder="Celebrations, redirections, or quick reminders"
              className="mt-2 w-full rounded-xl border border-primary-500/40 bg-slate-950/80 px-4 py-3 text-sm text-slate-100 placeholder:text-slate-500 focus:border-primary-400 focus:outline-none focus:ring-2 focus:ring-primary-400/40"
              disabled={submitting || students.length === 0}
            />
          </label>
          {error && <p className="text-xs text-rose-200">{error}</p>}
          {message && <p className="text-xs text-emerald-200">{message}</p>}
          <button
            type="button"
            onClick={handleSubmit}
            disabled={submitting || isSaving || students.length === 0}
            className="w-full rounded-xl bg-primary-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary-400 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {submitting || isSaving ? "Posting..." : "Add to behaviour log"}
          </button>
        </div>
      </div>
    </section>
  );
}
