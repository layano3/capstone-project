"use client";

import { useEffect, useMemo, useState } from "react";
import { Header } from "@/components/Header";
import { StatCards } from "@/components/StatCards";
import { ActivityOverview } from "@/components/ActivityOverview";
import { StudentTable } from "@/components/StudentTable";
import { StudentDetail } from "@/components/StudentDetail";
import { QuickActions } from "@/components/QuickActions";
import type { Student } from "@/types/student";

interface StudentsResponse {
  students: Student[];
  error?: string;
}

export default function Page() {
  const [roster, setRoster] = useState<Student[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [query, setQuery] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [savingIds, setSavingIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    async function fetchRoster() {
      setLoading(true);
      setError(null);
      try {
        const response = await fetch("/api/students");
        const payload = (await response.json()) as StudentsResponse;
        if (!response.ok) {
          throw new Error(payload.error ?? "Failed to load students");
        }
        setRoster(payload.students);
        if (payload.students.length > 0) {
          setSelectedId((prev) => prev ?? payload.students[0].id);
        }
      } catch (err) {
        console.error(err);
        setError(err instanceof Error ? err.message : "Failed to load students");
      } finally {
        setLoading(false);
      }
    }

    fetchRoster();
  }, []);

  const selectedStudent = useMemo(
    () => roster.find((student) => student.id === selectedId) ?? null,
    [roster, selectedId]
  );

  const handleSelect = (student: Student) => {
    setSelectedId(student.id);
  };

  const handleAdjustXp = async (student: Student, delta: number, reason: string) => {
    setSavingIds((prev) => new Set(prev).add(student.id));
    try {
      const response = await fetch(`/api/students/${student.id}/xp`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ delta, reason, updatedBy: "Ms. Seguin" }),
      });

      const payload = (await response.json()) as { student?: Student; error?: string };

      if (!response.ok || !payload.student) {
        throw new Error(payload.error ?? "Failed to update XP");
      }

      setRoster((prev) =>
        prev.map((item) => (item.id === payload.student!.id ? payload.student! : item))
      );
    } catch (err) {
      console.error(err);
      setError(err instanceof Error ? err.message : "Failed to update XP");
    } finally {
      setSavingIds((prev) => {
        const next = new Set(prev);
        next.delete(student.id);
        return next;
      });
    }
  };

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col gap-6 px-6 py-8">
        {error && (
          <div className="rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
            {error}
          </div>
        )}

        <StatCards students={roster} />

        <div className="grid gap-6 lg:grid-cols-[1.4fr_1fr]">
          <ActivityOverview students={roster} />
          <QuickActions />
        </div>

        <div className="grid gap-6 lg:grid-cols-[1.8fr_1fr]">
          <StudentTable
            students={roster}
            selectedId={selectedId}
            onSelect={handleSelect}
            onAdjustXp={handleAdjustXp}
            query={query}
            onQueryChange={setQuery}
            loading={loading}
            savingIds={savingIds}
          />
          <StudentDetail student={selectedStudent} />
        </div>
      </main>
    </div>
  );
}
