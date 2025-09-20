insert into public.students (id, name, grade, avatar_color, last_active, current_activity, activity_status, xp, xp_goal, time_played_minutes, streak_days, behaviour, next_checkpoint, guardians)
values
  ('00000000-0000-0000-0000-000000000001', 'Amelia Chen', 'Grade 5', '#6366F1', timezone('utc', now()) - interval '5 minutes', 'Temple of Fractions · Puzzle 3', 'puzzle', 1420, 1800, 565, 6, 'on-track', 'Unlock Crystal Vault', 'Lily & Patrick Chen'),
  ('00000000-0000-0000-0000-000000000002', 'Noah Patel', 'Grade 5', '#22C55E', timezone('utc', now()) - interval '25 minutes', 'Codebreaker Cavern · Boss', 'boss', 1685, 2000, 720, 10, 'on-track', 'Boss clear & feedback', 'Kavita & Arjun Patel'),
  ('00000000-0000-0000-0000-000000000003', 'Isla Martinez', 'Grade 5', '#F97316', timezone('utc', now()) - interval '16 hours', 'Forest of Factors · Exploration', 'exploring', 980, 1500, 410, 3, 'needs-support', 'Unlock factor trees', 'Sofia & Miguel Martinez'),
  ('00000000-0000-0000-0000-000000000004', 'Liam Okafor', 'Grade 5', '#A855F7', timezone('utc', now()) - interval '10 minutes', 'Skyward Equations · Puzzle 1', 'puzzle', 1195, 1600, 505, 5, 'on-track', 'Co-op puzzle unlock', 'Ada & Chukwuemeka Okafor'),
  ('00000000-0000-0000-0000-000000000005', 'Sienna Ross', 'Grade 5', '#E11D48', timezone('utc', now()) - interval '2 days', 'Main menu', 'offline', 730, 1200, 260, 1, 'needs-support', 'Return to Fraction Forest', 'Morgan Ross')
on conflict (id) do nothing;

insert into public.xp_events (student_id, delta, reason, updated_by, created_at)
values
  ('00000000-0000-0000-0000-000000000001', 40, 'Solved multi-step fraction puzzle', 'System', timezone('utc', now()) - interval '5 minutes'),
  ('00000000-0000-0000-0000-000000000001', -10, 'Redirected focus during class', 'Ms. Seguin', timezone('utc', now()) - interval '20 hours'),
  ('00000000-0000-0000-0000-000000000002', 75, 'Teamwork bonus', 'System', timezone('utc', now()) - interval '30 minutes'),
  ('00000000-0000-0000-0000-000000000002', 25, 'Helped a classmate troubleshoot', 'Ms. Seguin', timezone('utc', now()) - interval '2 days'),
  ('00000000-0000-0000-0000-000000000003', 35, 'Notebook reflection uploaded', 'System', timezone('utc', now()) - interval '20 hours'),
  ('00000000-0000-0000-0000-000000000003', -20, 'Late to session', 'Ms. Seguin', timezone('utc', now()) - interval '3 days'),
  ('00000000-0000-0000-0000-000000000004', 20, 'Participation bonus', 'System', timezone('utc', now()) - interval '10 minutes'),
  ('00000000-0000-0000-0000-000000000004', 15, 'Shared strategy insight', 'Ms. Seguin', timezone('utc', now()) - interval '4 days'),
  ('00000000-0000-0000-0000-000000000005', 30, 'Completed practice set at home', 'Guardian', timezone('utc', now()) - interval '2 days'),
  ('00000000-0000-0000-0000-000000000005', -15, 'Missed checkpoint feedback', 'Ms. Seguin', timezone('utc', now()) - interval '5 days')
on conflict do nothing;
