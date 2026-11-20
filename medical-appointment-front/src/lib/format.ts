// Appointment dates are calendar dates stored as "YYYY-MM-DDT00:00:00Z".
// Only the date part matters, so we never convert them across time zones.

export const DAY_NAMES = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
];

/** "2026-11-05T00:00:00Z" -> "2026-11-05" */
export function dateKey(value: string) {
  return value.slice(0, 10);
}

/** Local "today" as "YYYY-MM-DD" */
export function todayKey() {
  return new Date().toLocaleDateString("en-CA");
}

/** "YYYY-MM-DD" -> Date at local midnight (safe for display and day math) */
export function parseDateKey(key: string) {
  const [y, m, d] = key.slice(0, 10).split("-").map(Number);
  return new Date(y, m - 1, d);
}

export function toDateKey(date: Date) {
  return date.toLocaleDateString("en-CA");
}

export function addDays(key: string, days: number) {
  const date = parseDateKey(key);
  date.setDate(date.getDate() + days);
  return toDateKey(date);
}

/** "Thu, Nov 5, 2026" */
export function formatDate(value: string) {
  return parseDateKey(value).toLocaleDateString("en-US", {
    weekday: "short",
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

/** "Thursday, November 5" */
export function formatLongDate(value: string) {
  return parseDateKey(value).toLocaleDateString("en-US", {
    weekday: "long",
    month: "long",
    day: "numeric",
  });
}

/** "10:30:00" -> "10:30 AM" */
export function formatTime(value: string) {
  const [h, m] = value.split(":").map(Number);
  const suffix = h >= 12 ? "PM" : "AM";
  const hour12 = h % 12 === 0 ? 12 : h % 12;
  return `${hour12}:${String(m).padStart(2, "0")} ${suffix}`;
}

/** "10:30" -> "10:30:00" (inputs of type=time give HH:mm) */
export function toTimeValue(value: string) {
  return value.length === 5 ? `${value}:00` : value;
}

/** ISO timestamp -> "Nov 5, 2026" (real instants, e.g. uploadedAt) */
export function formatTimestamp(value: string) {
  return new Date(value).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function formatFileSize(bytes: number) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function initials(name?: string | null) {
  if (!name) return "?";
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

/** True when the appointment's date and time are already in the past (local time) */
export function isPast(dateValue: string, timeValue: string) {
  const date = parseDateKey(dateValue);
  const [h, m] = timeValue.split(":").map(Number);
  date.setHours(h, m, 0, 0);
  return date.getTime() < Date.now();
}
