import type { AppointmentStatus } from "@/types/appointment";

export const STATUS_VARIANT = {
  Scheduled: "outline",
  Confirmed: "default",
  Completed: "secondary",
  Cancelled: "destructive",
  NoShow: "outline",
} as const;

export const STATUS_LABEL: Record<AppointmentStatus, string> = {
  Scheduled: "Pending confirmation",
  Confirmed: "Confirmed",
  Completed: "Completed",
  Cancelled: "Cancelled",
  NoShow: "No-show",
};

export function statusVariant(status: string) {
  return STATUS_VARIANT[status as keyof typeof STATUS_VARIANT] ?? "outline";
}

export function statusLabel(status: string) {
  return STATUS_LABEL[status as AppointmentStatus] ?? status;
}

export function isActiveStatus(status: string) {
  return status === "Scheduled" || status === "Confirmed";
}
