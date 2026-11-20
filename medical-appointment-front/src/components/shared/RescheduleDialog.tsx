"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { rescheduleAppointment } from "@/lib/api/appointments";
import { formatDate, formatTime } from "@/lib/format";
import type { Appointment } from "@/types/appointment";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import SlotPicker from "./SlotPicker";

// The doctor moves an appointment to another free slot; the patient gets an email
export default function RescheduleDialog({ appointment }: { appointment: Appointment }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");

  const mutation = useMutation({
    mutationFn: () => rescheduleAppointment(appointment.id, { appointmentDate: date, appointmentTime: time }),
    onSuccess: () => {
      toast.success("Appointment rescheduled. The patient was notified.");
      queryClient.invalidateQueries({ queryKey: ["schedule"] });
      queryClient.invalidateQueries({ queryKey: ["available-slots", appointment.doctorId] });
      setOpen(false);
      setDate("");
      setTime("");
    },
  });

  return (
    <>
      <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
        Reschedule
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reschedule appointment</DialogTitle>
            <DialogDescription>
              {appointment.patientName ?? "Patient"} · currently {formatDate(appointment.appointmentDate)} at{" "}
              {formatTime(appointment.appointmentTime)}
            </DialogDescription>
          </DialogHeader>

          <SlotPicker
            doctorId={appointment.doctorId}
            date={date}
            time={time}
            onDateChange={setDate}
            onTimeChange={setTime}
          />

          {mutation.error && <p className="text-sm text-red-500">{mutation.error.message}</p>}

          <DialogFooter>
            <Button onClick={() => mutation.mutate()} disabled={!date || !time || mutation.isPending}>
              {mutation.isPending ? "Saving..." : "Move appointment"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
