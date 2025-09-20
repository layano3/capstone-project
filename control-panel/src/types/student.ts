export type ActivityStatus = "exploring" | "puzzle" | "boss" | "completed" | "offline";
export type BehaviourFlag = "on-track" | "needs-support" | "at-risk";

export interface XpEvent {
  id: string;
  timestamp: string;
  delta: number;
  reason: string;
  updatedBy: string;
}

export interface Student {
  id: string;
  name: string;
  avatarColor: string;
  grade: string;
  lastActive: string;
  currentActivity: string;
  activityStatus: ActivityStatus;
  xp: number;
  xpGoal: number;
  xpEvents: XpEvent[];
  timePlayedMinutes: number;
  streakDays: number;
  behaviour: BehaviourFlag;
  nextCheckpoint: string;
  guardians?: string;
}
