"use client";

import { useQuery } from "@tanstack/react-query";
import { getAvailableSlots } from "@/lib/api/appointments";
import { formatTime, todayKey } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ErrorState, LoadingState } from "./QueryState";

// Date + free time slots for a doctor. Slots come from the Appointment Service, which
// combines the doctor's weekly hours, time off and already-booked appointments.
export default function SlotPicker({
  doctorId,
  date,
  time,
  onDateChange,
  onTimeChange,
}: {
  doctorId: string;
  date: string;
  time: string;
  onDateChange: (date: string) => void;
  onTimeChange: (time: string) => void;
}) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["available-slots", doctorId, date],
    queryFn: () => getAvailableSlots(doctorId, date),
    enabled: !!date,
  });

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="slot-date">Date</Label>
        <Input
          id="slot-date"
          type="date"
          min={todayKey()}
          value={date}
          onChange={(e) => {
            onDateChange(e.target.value);
            onTimeChange("");
          }}
        />
      </div>

      {date && (
        <div className="space-y-2">
          <Label>Available times</Label>
          {isLoading && <LoadingState label="Checking availability..." />}
          {error && <ErrorState message={error.message} />}
          {data?.slots.length === 0 && (
            <p className="rounded-md bg-muted p-3 text-sm text-muted-foreground">
              No free times on this day. Please choose another date.
            </p>
          )}
          <div className="grid max-h-56 grid-cols-3 gap-2 overflow-y-auto sm:grid-cols-4">
            {data?.slots.map((slot) => (
              <Button
                key={slot}
                type="button"
                size="sm"
                variant={time === slot ? "default" : "outline"}
                aria-pressed={time === slot}
                onClick={() => onTimeChange(slot)}
              >
                {formatTime(slot)}
              </Button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
