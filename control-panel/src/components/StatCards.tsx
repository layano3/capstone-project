import { Award, Clock4, Flame, Users } from "lucide-react";
import type { Student } from "@/types/student";

interface StatCardsProps {
  students: Student[];
}

export function StatCards({ students }: StatCardsProps) {
  const totalXp = students.reduce((acc, student) => acc + student.xp, 0);
  const active = students.filter((student) => student.activityStatus !== "offline").length;
  const streakLeaders = students.filter((student) => student.streakDays >= 5).length;
  const needsSupport = students.filter((student) => student.behaviour !== "on-track").length;

  const cards = [
    {
      label: "Active explorers",
      value: `${active}/${students.length}`,
      change: "+2 vs yesterday",
      icon: Users,
    },
    {
      label: "Total class XP",
      value: totalXp.toLocaleString(),
      change: "+320 today",
      icon: Award,
    },
    {
      label: "Streak leaders",
      value: streakLeaders.toString(),
      change: "5+ day streak",
      icon: Flame,
    },
    {
      label: "Needs attention",
      value: needsSupport.toString(),
      change: "Review behaviour notes",
      icon: Clock4,
    },
  ];

  return (
    <section className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {cards.map((card) => (
        <article key={card.label} className="card p-6">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="text-xs uppercase tracking-[0.3em] text-slate-400">{card.label}</p>
              <p className="mt-3 text-2xl font-semibold text-slate-100">{card.value}</p>
              <p className="mt-2 text-xs font-medium text-primary-300">{card.change}</p>
            </div>
            <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary-500/15 text-primary-200">
              <card.icon className="h-6 w-6" />
            </span>
          </div>
        </article>
      ))}
    </section>
  );
}
