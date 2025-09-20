import { Menu, SquarePen, UsersRound } from "lucide-react";

export function Header() {
  return (
    <header className="flex items-center justify-between gap-4 border-b border-white/5 bg-slate-900/40 p-6">
      <div className="flex items-center gap-3">
        <span className="inline-flex h-11 w-11 items-center justify-center rounded-xl bg-primary-500/20 text-primary-200">
          <UsersRound className="h-6 w-6" />
        </span>
        <div>
          <p className="text-xs uppercase tracking-[0.2em] text-slate-400">MathQuest Escape</p>
          <h1 className="text-xl font-semibold text-slate-100">Teacher Control Center</h1>
        </div>
      </div>
      <div className="flex items-center gap-3 text-sm text-slate-300">
        <button className="hidden items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-2 transition hover:bg-white/10 sm:flex">
          <SquarePen className="h-4 w-4" />
          Quick note
        </button>
        <button className="inline-flex items-center justify-center rounded-full border border-white/10 bg-white/5 p-2 text-slate-300 transition hover:bg-white/10">
          <Menu className="h-5 w-5" />
        </button>
      </div>
    </header>
  );
}
