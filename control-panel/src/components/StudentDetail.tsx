import { Award, Clock, Flame, NotebookPen } from "lucide-react";
import type { Student } from "@/types/student";
import { formatMinutes, formatRelativeTime } from "@/lib/format";

interface StudentDetailProps {
  student: Student | null;
}

export function StudentDetail({ student }: StudentDetailProps) {
  if (!student) {
    return (
      <aside className="card h-full p-6">
        <div className="flex h-full flex-col items-center justify-center gap-3 text-center">
          <NotebookPen className="h-10 w-10 text-slate-500" />
          <p className="text-sm text-slate-400">Select a learner to review their timeline and notes.</p>
        </div>
      </aside>
    );
  }

  const progress = Math.min(100, Math.round((student.xp / student.xpGoal) * 100));

  return (
    <aside className="card flex h-full flex-col p-6">
      <header className="flex items-center gap-3">
        <span
          className="flex h-12 w-12 items-center justify-center rounded-full text-sm font-semibold text-white"
          style={{ backgroundColor: student.avatarColor }}
          aria-hidden
        >
          {student.name
            .split(" ")
            .map((token) => token[0])
            .join("")}
        </span>
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Student spotlight</p>
          <h3 className="text-lg font-semibold text-slate-100">{student.name}</h3>
          <p className="text-xs text-slate-500">{student.grade} · Guardians: {student.guardians ?? "—"}</p>
        </div>
      </header>

      <div className="mt-6 grid gap-4 sm:grid-cols-3">
        <Metric label="Total XP" value={student.xp.toLocaleString()} icon={Award} />
        <Metric label="Current streak" value={`${student.streakDays} days`} icon={Flame} />
        <Metric label="Playtime" value={formatMinutes(student.timePlayedMinutes)} icon={Clock} />
      </div>

      <div className="mt-6 rounded-2xl border border-white/5 bg-white/5 p-4">
        <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Progress toward goal</p>
        <div className="mt-3 flex items-end justify-between text-sm">
          <p className="font-semibold text-slate-100">{progress}%</p>
          <p className="text-slate-400">Next checkpoint: {student.nextCheckpoint}</p>
        </div>
        <div className="mt-4 h-2 w-full overflow-hidden rounded-full bg-slate-800">
          <span
            className="block h-full rounded-full bg-primary-500"
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>

      <section className="mt-6 flex-1 overflow-hidden">
        <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Recent XP events</p>
        <ul className="mt-3 space-y-3 overflow-y-auto pr-2">
          {student.xpEvents.map((event) => (
            <li key={event.id} className="rounded-2xl border border-white/5 bg-slate-900/60 p-4">
              <div className="flex items-center justify-between gap-3 text-sm">
                <p className="font-semibold text-slate-100">{event.reason}</p>
                <span
                  className="rounded-full px-3 py-1 text-xs font-semibold"
                  style={{
                    backgroundColor: event.delta >= 0 ? "rgba(34,197,94,0.15)" : "rgba(248,113,113,0.15)",
                    color: event.delta >= 0 ? "#4ADE80" : "#FCA5A5",
                  }}
                >
                  {event.delta >= 0 ? "+" : ""}
                  {event.delta} XP
                </span>
              </div>
              <div className="mt-2 flex items-center justify-between text-xs text-slate-400">
                <p>Logged by {event.updatedBy}</p>
                <p>{formatRelativeTime(event.timestamp)}</p>
              </div>
            </li>
          ))}
        </ul>
      </section>

      <footer className="mt-6 rounded-2xl border border-amber-500/20 bg-amber-500/10 p-4 text-xs text-amber-100">
        <p>
          Behaviour flag: <span className="font-semibold uppercase">{student.behaviour.replace("-", " ")}</span> — last check-in {formatRelativeTime(student.lastActive)}.
        </p>
      </footer>
    </aside>
  );
}

interface MetricProps {
  label: string;
  value: string;
  icon: React.ComponentType<{ className?: string }>;
}

function Metric({ label, value, icon: Icon }: MetricProps) {
  return (
    <div className="rounded-2xl border border-white/5 bg-slate-900/60 p-4">
      <div className="flex items-center gap-3">
        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-500/15 text-primary-200">
          <Icon className="h-5 w-5" />
        </span>
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">{label}</p>
          <p className="text-sm font-semibold text-slate-100">{value}</p>
        </div>
      </div>
    </div>
  );
}
