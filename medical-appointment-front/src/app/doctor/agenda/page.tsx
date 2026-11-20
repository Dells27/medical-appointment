"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { CalendarOff, ChevronLeft, ChevronRight } from "lucide-react";
import { getMySchedule } from "@/lib/api/appointments";
import { getMyTimeOff } from "@/lib/api/doctors";
import { statusLabel } from "@/lib/appointment-status";
import { addDays, dateKey, formatTime, parseDateKey, todayKey } from "@/lib/format";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import PageHeader from "@/components/shared/PageHeader";
import { ErrorState, LoadingState } from "@/components/shared/QueryState";

// Monday of the week that contains `key`
function startOfWeek(key: string) {
  const day = parseDateKey(key).getDay(); // 0 = Sunday
  return addDays(key, day === 0 ? -6 : 1 - day);
}

const STATUS_STYLE: Record<string, string> = {
  Scheduled: "border-l-amber-500 bg-amber-50 dark:bg-amber-500/10",
  Confirmed: "border-l-primary bg-primary/5",
  Completed: "border-l-muted-foreground/40 bg-muted/60 text-muted-foreground",
  NoShow: "border-l-muted-foreground/40 bg-muted/60 text-muted-foreground",
};

export default function AgendaPage() {
  const today = todayKey();
  const [weekStart, setWeekStart] = useState(() => startOfWeek(today));
  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ["schedule"],
    queryFn: getMySchedule,
  });
  const { data: timeOff } = useQuery({ queryKey: ["time-off"], queryFn: getMyTimeOff });

  const weekLabel = `${parseDateKey(days[0]).toLocaleDateString("en-US", { month: "short", day: "numeric" })} – ${parseDateKey(days[6]).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}`;

  return (
    <>
      <PageHeader
        title="My agenda"
        description={weekLabel}
        actions={
          <>
            <Button variant="outline" size="icon" aria-label="Previous week" onClick={() => setWeekStart(addDays(weekStart, -7))}>
              <ChevronLeft />
            </Button>
            <Button variant="outline" onClick={() => setWeekStart(startOfWeek(today))}>
              Today
            </Button>
            <Button variant="outline" size="icon" aria-label="Next week" onClick={() => setWeekStart(addDays(weekStart, 7))}>
              <ChevronRight />
            </Button>
          </>
        }
      />

      {isLoading && <LoadingState label="Loading agenda..." />}
      {error && <ErrorState message={error.message} />}

      <div className="grid gap-3 lg:grid-cols-7">
        {days.map((day) => {
          const items = (appointments ?? [])
            .filter((a) => dateKey(a.appointmentDate) === day && a.status !== "Cancelled")
            .sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime));
          const offs = (timeOff ?? []).filter((t) => t.startDate <= day && t.endDate >= day);
          const date = parseDateKey(day);
          const isToday = day === today;

          return (
            <section
              key={day}
              className={cn("min-h-32 rounded-lg border bg-card p-2", isToday && "border-primary ring-1 ring-primary/30")}
              aria-label={date.toDateString()}
            >
              <header className="mb-2 flex items-baseline justify-between px-1">
                <span className="text-xs font-medium text-muted-foreground uppercase">
                  {date.toLocaleDateString("en-US", { weekday: "short" })}
                </span>
                <span className={cn("text-lg font-semibold tabular-nums", isToday && "text-primary")}>
                  {date.getDate()}
                </span>
              </header>

              <div className="space-y-1.5">
                {offs.map((t) => (
                  <div key={t.id} className="flex items-center gap-1.5 rounded-md bg-muted px-2 py-1 text-xs text-muted-foreground">
                    <CalendarOff className="size-3 shrink-0" />
                    <span className="truncate">
                      {t.type === "BlockedHours" && t.startTime && t.endTime
                        ? `${formatTime(t.startTime)}–${formatTime(t.endTime)}`
                        : "Off"}
                      {t.reason ? ` · ${t.reason}` : ""}
                    </span>
                  </div>
                ))}

                {items.map((a) => (
                  <Link
                    key={a.id}
                    href={`/doctor/patients/${a.patientId}`}
                    className={cn(
                      "block rounded-md border-l-4 px-2 py-1.5 text-xs transition-opacity hover:opacity-80",
                      STATUS_STYLE[a.status],
                    )}
                    title={statusLabel(a.status)}
                  >
                    <span className="block font-medium tabular-nums">{formatTime(a.appointmentTime)}</span>
                    <span className="block truncate">{a.patientName ?? "Patient"}</span>
                  </Link>
                ))}

                {items.length === 0 && offs.length === 0 && (
                  <p className="px-1 text-xs text-muted-foreground/70">No appointments</p>
                )}
              </div>
            </section>
          );
        })}
      </div>

      <div className="flex flex-wrap gap-4 text-xs text-muted-foreground">
        <span className="flex items-center gap-1.5"><span className="size-2.5 rounded-sm bg-amber-500" /> Pending confirmation</span>
        <span className="flex items-center gap-1.5"><span className="size-2.5 rounded-sm bg-primary" /> Confirmed</span>
        <span className="flex items-center gap-1.5"><span className="size-2.5 rounded-sm bg-muted-foreground/40" /> Completed</span>
      </div>
    </>
  );
}
