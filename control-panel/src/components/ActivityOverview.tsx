import { ActivityStatus, Student } from "@/types/student";

interface Props {
  students: Student[];
}

const statusPalette: Record<ActivityStatus, string> = {
  exploring: "#34d399",
  puzzle: "#38bdf8",
  boss: "#facc15",
  completed: "#6366f1",
  offline: "#94a3b8",
};

export function ActivityOverview({ students }: Props) {
  const counts = students.reduce<Record<ActivityStatus, number>>((acc, student) => {
    acc[student.activityStatus] += 1;
    return acc;
  }, {
    exploring: 0,
    puzzle: 0,
    boss: 0,
    completed: 0,
    offline: 0,
  });

  const total = students.length;
  const entries = (Object.entries(counts) as [ActivityStatus, number][]).filter(([, value]) => value > 0);

  return (
    <section className="card p-6">
      <header className="flex items-start justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Session heatmap</p>
          <h2 className="mt-2 text-lg font-semibold text-slate-100">Where learners are now</h2>
        </div>
        <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-400">
          Updated {new Date().toLocaleTimeString([], { hour: "numeric", minute: "2-digit" })}
        </span>
      </header>
      <div className="mt-6 grid gap-6 lg:grid-cols-[1.5fr_1fr]">
        <div className="flex items-center justify-center">
          <div className="relative h-44 w-44">
            {total > 0 ? (
              <svg viewBox="0 0 36 36" className="h-full w-full -rotate-90">
                {entries.reduce<{ start: number; elements: JSX.Element[] }>((acc, [status, value]) => {
                  const percent = value / total;
                  const dashArray = `${percent * 100} ${(1 - percent) * 100}`;
                  const circle = (
                    <circle
                      key={status}
                      cx="18"
                      cy="18"
                      r="16"
                      fill="transparent"
                      stroke={statusPalette[status]}
                      strokeWidth="3.2"
                      strokeDasharray={dashArray}
                      strokeDashoffset={acc.start}
                      strokeLinecap="round"
                      opacity="0.9"
                    />
                  );
                  const nextStart = acc.start - percent * 100;
                  return {
                    start: nextStart,
                    elements: [...acc.elements, circle],
                  };
                }, { start: 25, elements: [] as JSX.Element[] }).elements}
              </svg>
            ) : (
              <div className="flex h-full w-full items-center justify-center rounded-full border border-dashed border-white/10 text-sm text-slate-500">
                No active sessions
              </div>
            )}
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-1 text-center">
              <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Total</p>
              <p className="text-3xl font-semibold text-slate-100">{total}</p>
              <p className="text-xs text-slate-500">Learners</p>
            </div>
          </div>
        </div>
        <ul className="space-y-3">
          {entries.map(([status, value]) => (
            <li key={status} className="flex items-center justify-between gap-3 rounded-xl border border-white/5 bg-slate-900/60 px-4 py-3">
              <div className="flex items-center gap-3">
                <span className="inline-flex h-9 w-9 items-center justify-center rounded-full" style={{ backgroundColor: `${statusPalette[status]}20` }}>
                  <span className="h-3 w-3 rounded-full" style={{ backgroundColor: statusPalette[status] }} />
                </span>
                <div>
                  <p className="text-sm font-semibold text-slate-100">{statusLabel(status)}</p>
                  <p className="text-xs text-slate-500">{total > 0 ? Math.round((value / total) * 100) : 0}% of class</p>
                </div>
              </div>
              <span className="text-lg font-semibold text-slate-100">{value}</span>
            </li>
          ))}
          {entries.length === 0 && (
            <li className="rounded-xl border border-dashed border-white/10 px-4 py-6 text-sm text-slate-500">
              No session data recorded yet.
            </li>
          )}
        </ul>
      </div>
    </section>
  );
}

function statusLabel(status: ActivityStatus) {
  switch (status) {
    case "exploring":
      return "Exploring zones";
    case "puzzle":
      return "Puzzle rooms";
    case "boss":
      return "Boss encounters";
    case "completed":
      return "Session complete";
    case "offline":
      return "Offline";
    default:
      return status;
  }
}
