"use client";

import { useMemo, useState } from "react";
import { ArrowUpRight, BadgeInfo, ChevronRight, MinusCircle, PlusCircle } from "lucide-react";
import clsx from "clsx";
import type { Student } from "@/types/student";
import { formatRelativeTime } from "@/lib/format";

interface StudentTableProps {
  students: Student[];
  selectedId: string | null;
  onSelect: (student: Student) => void;
  onAdjustXp: (student: Student, delta: number, reason: string) => Promise<void> | void;
  query: string;
  onQueryChange: (value: string) => void;
  loading?: boolean;
  savingIds: Set<string>;
}

type QuickOption = {
  label: string;
  value: number;
};

const quickAwards: QuickOption[] = [
  { label: "+10", value: 10 },
  { label: "+25", value: 25 },
  { label: "+50", value: 50 },
];

const quickDeductions: QuickOption[] = [
  { label: "-10", value: -10 },
  { label: "-25", value: -25 },
];

export function StudentTable({
  students,
  selectedId,
  onSelect,
  onAdjustXp,
  query,
  onQueryChange,
  loading = false,
  savingIds,
}: StudentTableProps) {
  const [adjustingId, setAdjustingId] = useState<string | null>(null);
  const [delta, setDelta] = useState<number>(25);
  const [reason, setReason] = useState<string>("Recognized participation");
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState<boolean>(false);

  const filtered = useMemo(() => {
    const lower = query.toLowerCase();
    return students.filter((student) =>
      [student.name, student.grade, student.currentActivity]
        .join(" ")
        .toLowerCase()
        .includes(lower)
    );
  }, [students, query]);

  const toggleAdjust = (id: string) => {
    setSubmitError(null);
    setReason("Recognized participation");
    setDelta(25);
    setAdjustingId((prev) => (prev === id ? null : id));
  };

  const applyAdjustment = async (student: Student) => {
    if (submitting) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      await onAdjustXp(student, delta, reason.trim() || "Manual adjustment");
      setAdjustingId(null);
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : "Failed to apply change");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="card p-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-100">Student roster</h2>
          <p className="text-sm text-slate-400">
            Track live progress, award XP and capture coaching notes.
          </p>
        </div>
        <input
          value={query}
          onChange={(event) => onQueryChange(event.target.value)}
          placeholder="Search by name, grade, or activity..."
          className="w-full rounded-xl border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:border-primary-400 focus:outline-none focus:ring-2 focus:ring-primary-400/40 sm:w-72"
        />
      </div>
      <div className="mt-6 overflow-hidden rounded-2xl border border-white/5">
        <table className="min-w-full divide-y divide-white/5">
          <thead className="bg-white/5">
            <tr className="text-left text-xs uppercase tracking-[0.2em] text-slate-400">
              <th className="px-6 py-4 font-medium">Student</th>
              <th className="px-6 py-4 font-medium">Activity</th>
              <th className="px-6 py-4 font-medium">XP</th>
              <th className="px-6 py-4 font-medium">Last active</th>
              <th className="px-6 py-4 font-medium">Status</th>
              <th className="px-6 py-4 font-medium">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {loading && (
              <tr>
                <td colSpan={6} className="px-6 py-8 text-center text-sm text-slate-400">
                  Loading roster...
                </td>
              </tr>
            )}
            {!loading && filtered.map((student) => {
              const progress = student.xpGoal > 0 ? Math.min(100, Math.round((student.xp / student.xpGoal) * 100)) : 0;
              const isSelected = selectedId === student.id;
              const isSaving = savingIds.has(student.id);

              return (
                <tr
                  key={student.id}
                  className={clsx(
                    "bg-slate-900/40 transition hover:bg-slate-900/70",
                    isSelected && "bg-primary-500/10"
                  )}
                >
                  <td className="whitespace-nowrap px-6 py-4">
                    <button
                      type="button"
                      onClick={() => onSelect(student)}
                      className="flex items-center gap-4 text-left"
                    >
                      <span
                        className="flex h-11 w-11 items-center justify-center rounded-full text-sm font-semibold text-white"
                        style={{ backgroundColor: student.avatarColor }}
                        aria-hidden
                      >
                        {student.name
                          .split(" ")
                          .map((token) => token[0])
                          .join("")}
                      </span>
                      <span>
                        <p className="font-semibold text-slate-100">{student.name}</p>
                        <p className="text-xs text-slate-500">{student.grade}</p>
                      </span>
                    </button>
                  </td>
                  <td className="max-w-sm px-6 py-4">
                    <p className="text-sm text-slate-200">{student.currentActivity}</p>
                    <p className="mt-1 text-xs text-slate-500">
                      Next: {student.nextCheckpoint}
                    </p>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex flex-col gap-1 text-sm">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-slate-100">{student.xp}</span>
                        <span className="text-xs text-slate-500">goal {student.xpGoal}</span>
                      </div>
                      <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-800">
                        <span
                          className="block h-full rounded-full bg-primary-500"
                          style={{ width: `${progress}%` }}
                        />
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <p className="text-sm text-slate-200">{formatRelativeTime(student.lastActive)}</p>
                    <p className="mt-1 text-xs text-slate-500">
                      {Math.round(student.timePlayedMinutes / 60)}h total
                    </p>
                  </td>
                  <td className="px-6 py-4">
                    <StatusBadge status={student.activityStatus} behaviour={student.behaviour} />
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        type="button"
                        onClick={() => toggleAdjust(student.id)}
                        className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-primary-500/40 hover:bg-primary-500/10"
                        disabled={isSaving}
                      >
                        <PlusCircle className="h-4 w-4" /> XP
                      </button>
                      <button
                        type="button"
                        onClick={() => onSelect(student)}
                        className="inline-flex items-center gap-1 rounded-full border border-transparent px-2 py-2 text-xs text-slate-400 transition hover:text-primary-300"
                      >
                        View
                        <ChevronRight className="h-3 w-3" />
                      </button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {!loading && filtered.length === 0 && (
        <div className="flex flex-col items-center justify-center gap-3 py-16 text-center">
          <BadgeInfo className="h-10 w-10 text-slate-500" />
          <p className="text-sm text-slate-400">No students match your filters.</p>
        </div>
      )}

      {adjustingId && (
        <div className="mt-6 rounded-2xl border border-primary-500/30 bg-primary-500/10 p-6">
          {(() => {
            const student = students.find((item) => item.id === adjustingId);
            if (!student) return null;
            const preview = student.xp + delta;
            const isSaving = savingIds.has(student.id) || submitting;

            return (
              <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                <div className="flex-1">
                  <p className="text-xs uppercase tracking-[0.3em] text-primary-200">
                    Adjust XP · {student.name}
                  </p>
                  <h3 className="mt-2 text-2xl font-semibold text-slate-100">
                    New total preview: {preview < 0 ? 0 : preview}
                  </h3>
                  <p className="mt-2 text-sm text-slate-200">
                    Capture a quick justification so the student sees the context in their activity feed.
                  </p>
                  <textarea
                    value={reason}
                    onChange={(event) => setReason(event.target.value)}
                    rows={3}
                    className="mt-4 w-full rounded-xl border border-primary-500/40 bg-slate-950/80 px-4 py-3 text-sm text-slate-100 placeholder:text-slate-500 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-400/40"
                    placeholder="Reason visible to the learner and guardians"
                  />
                  {submitError && (
                    <p className="mt-3 text-xs text-rose-200">{submitError}</p>
                  )}
                </div>
                <div className="flex w-full flex-col gap-4 lg:w-72">
                  <div>
                    <p className="text-xs uppercase tracking-[0.3em] text-primary-200">Quick award</p>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {quickAwards.map((option) => (
                        <button
                          key={option.label}
                          type="button"
                          onClick={() => setDelta(option.value)}
                          className={clsx(
                            "inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-sm transition",
                            delta === option.value
                              ? "border-primary-400 bg-primary-500/20 text-primary-100"
                              : "border-white/10 bg-white/5 text-slate-200 hover:border-primary-400/40 hover:text-primary-100"
                          )}
                          disabled={isSaving}
                        >
                          <PlusCircle className="h-4 w-4" />
                          {option.label}
                        </button>
                      ))}
                    </div>
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-[0.3em] text-primary-200">Quick deduction</p>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {quickDeductions.map((option) => (
                        <button
                          key={option.label}
                          type="button"
                          onClick={() => setDelta(option.value)}
                          className={clsx(
                            "inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-sm transition",
                            delta === option.value
                              ? "border-primary-400 bg-primary-500/20 text-primary-100"
                              : "border-white/10 bg-white/5 text-slate-200 hover:border-primary-400/40 hover:text-primary-100"
                          )}
                          disabled={isSaving}
                        >
                          <MinusCircle className="h-4 w-4" />
                          {option.label}
                        </button>
                      ))}
                    </div>
                  </div>
                  <div>
                    <label className="flex flex-col gap-1 text-xs font-semibold uppercase tracking-[0.3em] text-primary-200">
                      Custom delta
                      <input
                        type="number"
                        value={delta}
                        onChange={(event) => setDelta(Number(event.target.value))}
                        className="rounded-xl border border-primary-500/40 bg-slate-950/80 px-4 py-2 text-sm font-medium text-slate-100 focus:border-primary-400 focus:outline-none focus:ring-2 focus:ring-primary-400/40"
                        disabled={isSaving}
                      />
                    </label>
                  </div>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => applyAdjustment(student)}
                      className="flex-1 rounded-xl bg-primary-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary-400 disabled:cursor-not-allowed disabled:opacity-70"
                      disabled={isSaving}
                    >
                      {isSaving ? "Saving..." : "Apply change"}
                    </button>
                    <button
                      type="button"
                      onClick={() => setAdjustingId(null)}
                      className="flex-1 rounded-xl border border-white/10 bg-white/5 px-4 py-2 text-sm font-semibold text-slate-200 transition hover:border-white/20 hover:bg-white/10"
                      disabled={isSaving}
                    >
                      Cancel
                    </button>
                  </div>
                  <p className="text-xs text-slate-400">
                    <ArrowUpRight className="mr-1 inline h-3.5 w-3.5" /> Changes sync to MathQuest once integrations are enabled.
                  </p>
                </div>
              </div>
            );
          })()}
        </div>
      )}
    </section>
  );
}

interface StatusBadgeProps {
  status: Student["activityStatus"];
  behaviour: Student["behaviour"];
}

const statusCopy: Record<StatusBadgeProps["status"], string> = {
  exploring: "Exploring",
  puzzle: "Puzzle",
  boss: "Boss battle",
  completed: "Completed",
  offline: "Offline",
};

function StatusBadge({ status, behaviour }: StatusBadgeProps) {
  const color = {
    exploring: "bg-emerald-500/15 text-emerald-200 border-emerald-500/30",
    puzzle: "bg-cyan-500/15 text-cyan-200 border-cyan-500/30",
    boss: "bg-amber-500/15 text-amber-200 border-amber-500/30",
    completed: "bg-primary-500/15 text-primary-200 border-primary-500/30",
    offline: "bg-slate-500/20 text-slate-200 border-slate-500/40",
  }[status];

  const behaviourCopy = {
    "on-track": "On track",
    "needs-support": "Coach soon",
    "at-risk": "Immediate action",
  }[behaviour];

  return (
    <div className="flex flex-col gap-2">
      <span className={clsx("inline-flex w-fit items-center gap-2 rounded-full border px-3 py-1 text-xs font-semibold", color)}>
        {statusCopy[status]}
      </span>
      {behaviour !== "on-track" && (
        <span className="inline-flex items-center gap-1 text-xs font-medium text-amber-300">
          ⚠️ {behaviourCopy}
        </span>
      )}
    </div>
  );
}
