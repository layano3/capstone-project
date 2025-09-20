import { ClipboardList, FileSpreadsheet, Send } from "lucide-react";

export function QuickActions() {
  const actions = [
    {
      label: "Export snapshot",
      description: "Download CSV of current XP and progress",
      icon: FileSpreadsheet,
      href: "#",
    },
    {
      label: "Message guardians",
      description: "Send weekly digest to selected families",
      icon: Send,
      href: "#",
    },
    {
      label: "Log behaviour notes",
      description: "Record classroom celebrations or redirections",
      icon: ClipboardList,
      href: "#",
    },
  ];

  return (
    <section className="card p-6">
      <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Quick actions</p>
      <ul className="mt-4 space-y-3">
        {actions.map((action) => (
          <li key={action.label}>
            <a
              href={action.href}
              className="flex items-center gap-3 rounded-2xl border border-white/5 bg-white/5 px-4 py-3 transition hover:border-primary-400/40 hover:bg-primary-500/10"
            >
              <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-500/15 text-primary-200">
                <action.icon className="h-5 w-5" />
              </span>
              <div>
                <p className="text-sm font-semibold text-slate-100">{action.label}</p>
                <p className="text-xs text-slate-500">{action.description}</p>
              </div>
            </a>
          </li>
        ))}
      </ul>
    </section>
  );
}
