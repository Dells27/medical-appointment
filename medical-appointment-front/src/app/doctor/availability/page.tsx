"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { CalendarOff, Plus, Trash2 } from "lucide-react";
import {
  addTimeOff,
  deleteTimeOff,
  getMyDoctorProfile,
  getMyTimeOff,
  replaceMySchedules,
} from "@/lib/api/doctors";
import { DAY_NAMES, formatDate, formatTime, todayKey, toTimeValue } from "@/lib/format";
import type { Doctor, ScheduleInput, TimeOff, TimeOffType } from "@/types/doctor";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select";
import { Switch } from "@/components/ui/switch";
import PageHeader from "@/components/shared/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

// Monday first, as most agendas show it
const WEEK = [1, 2, 3, 4, 5, 6, 0];

interface Block {
  start: string; // "08:00"
  end: string;
}

export default function AvailabilityPage() {
  const { data: profile } = useQuery({ queryKey: ["doctor-profile"], queryFn: getMyDoctorProfile });

  return (
    <>
      <PageHeader
        title="Schedule & availability"
        description="Patients can only book inside your working hours and outside your time off."
      />
      {profile ? <WeeklyHours key={profile.id} profile={profile} /> : <LoadingState />}
      <TimeOffSection />
    </>
  );
}

// ---------------------------------------------------------------------------
// Weekly working hours
// ---------------------------------------------------------------------------

function WeeklyHours({ profile }: { profile: Doctor }) {
  const queryClient = useQueryClient();

  const [days, setDays] = useState<Record<number, Block[]>>(() => {
    const initial: Record<number, Block[]> = Object.fromEntries(WEEK.map((d) => [d, []]));
    for (const s of profile.schedules) {
      initial[s.dayOfWeek].push({ start: s.startTime.slice(0, 5), end: s.endTime.slice(0, 5) });
    }
    return initial;
  });

  const save = useMutation({
    mutationFn: () => {
      const schedules: ScheduleInput[] = WEEK.flatMap((day) =>
        days[day].map((b) => ({
          dayOfWeek: day,
          startTime: toTimeValue(b.start),
          endTime: toTimeValue(b.end),
        })),
      );
      return replaceMySchedules(schedules);
    },
    onSuccess: (schedules) => {
      toast.success("Working hours saved");
      queryClient.setQueryData<Doctor>(["doctor-profile"], (old) => (old ? { ...old, schedules } : old));
      queryClient.invalidateQueries({ queryKey: ["available-slots"] });
      queryClient.invalidateQueries({ queryKey: ["public-doctors"] });
    },
  });

  const setDay = (day: number, blocks: Block[]) => setDays((d) => ({ ...d, [day]: blocks }));

  const invalid = WEEK.some((day) => days[day].some((b) => !b.start || !b.end || b.start >= b.end));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Weekly working hours</CardTitle>
        <CardDescription>Appointments are offered in 30-minute slots within these hours.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-1">
        {WEEK.map((day) => {
          const blocks = days[day];
          const enabled = blocks.length > 0;
          return (
            <div
              key={day}
              className="flex flex-col gap-3 border-b py-3 last:border-b-0 sm:flex-row sm:items-start"
            >
              <label className="flex w-40 shrink-0 items-center gap-3 pt-1.5 text-sm font-medium">
                <Switch
                  checked={enabled}
                  onCheckedChange={(on) => setDay(day, on ? [{ start: "09:00", end: "17:00" }] : [])}
                />
                {DAY_NAMES[day]}
              </label>

              <div className="flex-1 space-y-2">
                {!enabled && <p className="pt-1.5 text-sm text-muted-foreground">Not available</p>}
                {blocks.map((block, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <Input
                      type="time"
                      step={1800}
                      className="w-32"
                      value={block.start}
                      aria-label={`${DAY_NAMES[day]} start`}
                      onChange={(e) =>
                        setDay(day, blocks.map((b, j) => (j === i ? { ...b, start: e.target.value } : b)))
                      }
                    />
                    <span className="text-sm text-muted-foreground">to</span>
                    <Input
                      type="time"
                      step={1800}
                      className="w-32"
                      value={block.end}
                      aria-label={`${DAY_NAMES[day]} end`}
                      onChange={(e) =>
                        setDay(day, blocks.map((b, j) => (j === i ? { ...b, end: e.target.value } : b)))
                      }
                    />
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Remove time range"
                      onClick={() => setDay(day, blocks.filter((_, j) => j !== i))}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                ))}
                {enabled && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setDay(day, [...blocks, { start: "14:00", end: "18:00" }])}
                  >
                    <Plus />
                    Add range
                  </Button>
                )}
              </div>
            </div>
          );
        })}

        {invalid && (
          <p className="pt-2 text-sm text-red-500">Each range must end after it starts.</p>
        )}
        {save.error && <p className="pt-2 text-sm text-red-500">{save.error.message}</p>}

        <div className="pt-4">
          <Button onClick={() => save.mutate()} disabled={invalid || save.isPending}>
            {save.isPending ? "Saving..." : "Save working hours"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// ---------------------------------------------------------------------------
// Time off: vacation, days off, blocked hours
// ---------------------------------------------------------------------------

const TYPE_LABEL: Record<TimeOffType, string> = {
  Vacation: "Vacation",
  DayOff: "Day off",
  BlockedHours: "Blocked hours",
};

function describe(t: TimeOff) {
  if (t.type === "BlockedHours" && t.startTime && t.endTime) {
    return `${formatDate(t.startDate)} · ${formatTime(t.startTime)} – ${formatTime(t.endTime)}`;
  }
  return t.startDate === t.endDate
    ? formatDate(t.startDate)
    : `${formatDate(t.startDate)} – ${formatDate(t.endDate)}`;
}

function TimeOffSection() {
  const queryClient = useQueryClient();
  const today = todayKey();

  const [type, setType] = useState<TimeOffType>("Vacation");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [startTime, setStartTime] = useState("12:00");
  const [endTime, setEndTime] = useState("14:00");
  const [reason, setReason] = useState("");

  const { data: timeOff, isLoading, error } = useQuery({
    queryKey: ["time-off"],
    queryFn: getMyTimeOff,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["time-off"] });
    queryClient.invalidateQueries({ queryKey: ["available-slots"] });
  };

  const create = useMutation({
    mutationFn: () =>
      addTimeOff({
        type,
        startDate,
        endDate: type === "BlockedHours" ? startDate : endDate || startDate,
        startTime: type === "BlockedHours" ? toTimeValue(startTime) : undefined,
        endTime: type === "BlockedHours" ? toTimeValue(endTime) : undefined,
        reason: reason.trim() || undefined,
      }),
    onSuccess: () => {
      toast.success("Time off added");
      invalidate();
      setStartDate("");
      setEndDate("");
      setReason("");
    },
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteTimeOff(id),
    onSuccess: () => {
      toast.success("Time off removed");
      invalidate();
    },
    onError: (err) => toast.error(err.message),
  });

  // Past entries are hidden: they no longer affect booking
  const upcoming = (timeOff ?? []).filter((t) => t.endDate >= today);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Time off</CardTitle>
        <CardDescription>
          Block vacations, days off or specific hours. Existing appointments are not cancelled
          automatically — reschedule or cancel them from your appointments.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <form
          className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4"
          onSubmit={(e) => {
            e.preventDefault();
            create.mutate();
          }}
        >
          <div className="space-y-2">
            <Label htmlFor="to-type">Type</Label>
            <NativeSelect
              id="to-type"
              className="w-full"
              value={type}
              onChange={(e) => setType(e.target.value as TimeOffType)}
            >
              {Object.entries(TYPE_LABEL).map(([value, label]) => (
                <NativeSelectOption key={value} value={value}>
                  {label}
                </NativeSelectOption>
              ))}
            </NativeSelect>
          </div>

          <div className="space-y-2">
            <Label htmlFor="to-start">{type === "BlockedHours" ? "Date" : "From"}</Label>
            <Input
              id="to-start"
              type="date"
              min={today}
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              required
            />
          </div>

          {type === "BlockedHours" ? (
            <div className="space-y-2">
              <Label>Hours</Label>
              <div className="flex items-center gap-2">
                <Input type="time" step={1800} value={startTime} onChange={(e) => setStartTime(e.target.value)} aria-label="Start time" />
                <Input type="time" step={1800} value={endTime} onChange={(e) => setEndTime(e.target.value)} aria-label="End time" />
              </div>
            </div>
          ) : (
            <div className="space-y-2">
              <Label htmlFor="to-end">To</Label>
              <Input
                id="to-end"
                type="date"
                min={startDate || today}
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                placeholder="Same day"
              />
            </div>
          )}

          <div className="space-y-2">
            <Label htmlFor="to-reason">Reason (optional)</Label>
            <Input id="to-reason" maxLength={500} value={reason} onChange={(e) => setReason(e.target.value)} />
          </div>

          {create.error && <p className="text-sm text-red-500 sm:col-span-2 lg:col-span-4">{create.error.message}</p>}

          <div className="sm:col-span-2 lg:col-span-4">
            <Button type="submit" disabled={!startDate || create.isPending}>
              <CalendarOff />
              {create.isPending ? "Adding..." : "Add time off"}
            </Button>
          </div>
        </form>

        {isLoading && <LoadingState />}
        {error && <ErrorState message={error.message} />}
        {timeOff && upcoming.length === 0 && (
          <EmptyState icon={CalendarOff} title="No upcoming time off" />
        )}

        <div className="space-y-2">
          {upcoming.map((t) => (
            <div key={t.id} className="flex items-center justify-between gap-3 rounded-md border p-3">
              <div className="flex min-w-0 items-center gap-3">
                <Badge variant="outline">{TYPE_LABEL[t.type]}</Badge>
                <div className="min-w-0">
                  <p className="text-sm font-medium">{describe(t)}</p>
                  {t.reason && <p className="truncate text-xs text-muted-foreground">{t.reason}</p>}
                </div>
              </div>
              <Button
                variant="ghost"
                size="icon-sm"
                aria-label="Remove time off"
                disabled={remove.isPending}
                onClick={() => remove.mutate(t.id)}
              >
                <Trash2 />
              </Button>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
