export function formatRelativeTime(timestamp: string) {
  const time = new Date(timestamp).getTime();
  const now = Date.now();
  const diff = Math.max(0, now - time);

  const minute = 60 * 1000;
  const hour = 60 * minute;
  const day = 24 * hour;

  if (diff < minute) return "just now";
  if (diff < hour) {
    const value = Math.floor(diff / minute);
    return `${value} min ago`;
  }
  if (diff < day) {
    const value = Math.floor(diff / hour);
    return `${value} hr${value === 1 ? "" : "s"} ago`;
  }
  const value = Math.floor(diff / day);
  return `${value} day${value === 1 ? "" : "s"} ago`;
}

export function formatMinutes(minutes: number) {
  const hrs = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return hrs > 0 ? `${hrs}h ${mins}m` : `${mins}m`;
}
