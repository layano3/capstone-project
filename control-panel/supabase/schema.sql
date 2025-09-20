-- Core tables for MathQuest teacher console
create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";

create table if not exists public.students (
  id uuid primary key default gen_random_uuid(),
  name text not null,
  grade text not null,
  avatar_color text not null,
  last_active timestamptz not null default timezone('utc', now()),
  current_activity text not null,
  activity_status text not null check (activity_status in ('exploring','puzzle','boss','completed','offline')),
  xp integer not null default 0,
  xp_goal integer not null default 1000,
  time_played_minutes integer not null default 0,
  streak_days integer not null default 0,
  behaviour text not null default 'on-track' check (behaviour in ('on-track','needs-support','at-risk')),
  next_checkpoint text,
  guardians text,
  created_at timestamptz not null default timezone('utc', now()),
  updated_at timestamptz not null default timezone('utc', now())
);

create table if not exists public.xp_events (
  id uuid primary key default gen_random_uuid(),
  student_id uuid not null references public.students (id) on delete cascade,
  delta integer not null,
  reason text not null,
  updated_by text not null,
  created_at timestamptz not null default timezone('utc', now())
);

create index if not exists idx_students_activity_status on public.students(activity_status);
create index if not exists idx_students_last_active on public.students(last_active desc);
create index if not exists idx_xp_events_student_id_created_at on public.xp_events(student_id, created_at desc);

alter table public.students enable row level security;
alter table public.xp_events enable row level security;

create policy "Public read students" on public.students for select using (true);
create policy "Public read xp events" on public.xp_events for select using (true);
create policy "Service role full access students" on public.students for all using (auth.role() = 'service_role') with check (auth.role() = 'service_role');
create policy "Service role full access xp_events" on public.xp_events for all using (auth.role() = 'service_role') with check (auth.role() = 'service_role');

create function public.set_timestamp()
returns trigger language plpgsql as $$
begin
  NEW.updated_at = timezone('utc', now());
  return NEW;
end;
$$;

drop trigger if exists set_timestamp_students on public.students;
create trigger set_timestamp_students
before update on public.students
for each row execute function public.set_timestamp();
