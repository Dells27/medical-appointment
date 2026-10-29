export const STATUS_VARIANT = {
  Scheduled: "default",
  Confirmed: "default",
  Completed: "secondary",
  Cancelled: "destructive",
  NoShow: "outline",
} as const;

export function statusVariant(status: string) {
  return STATUS_VARIANT[status as keyof typeof STATUS_VARIANT] ?? "outline";
}