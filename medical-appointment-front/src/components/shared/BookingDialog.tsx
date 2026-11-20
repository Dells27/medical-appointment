"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { CalendarPlus } from "lucide-react";
import { createAppointment } from "@/lib/api/appointments";
import type { PublicDoctor } from "@/types/doctor";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import SlotPicker from "./SlotPicker";

export default function BookingDialog({ doctor }: { doctor: PublicDoctor }) {
  const queryClient = useQueryClient();

  const [open, setOpen] = useState(false);
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");
  const [reason, setReason] = useState("");

  function reset() {
    setDate("");
    setTime("");
    setReason("");
  }

  const mutation = useMutation({
    mutationFn: () =>
      createAppointment({
        doctorId: doctor.userId, // appointments store the doctor's user id
        appointmentDate: date,
        appointmentTime: time,
        reason: reason.trim() || undefined,
      }),
    onSuccess: () => {
      toast.success("Appointment booked. Your doctor will confirm it soon.");
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      setOpen(false);
      reset();
    },
    // The slot may have been taken meanwhile: refresh the list
    onError: () => queryClient.invalidateQueries({ queryKey: ["available-slots", doctor.userId] }),
  });

  return (
    <>
      <Button className="w-full" onClick={() => setOpen(true)}>
        <CalendarPlus />
        Book appointment
      </Button>

      <Dialog
        open={open}
        onOpenChange={(next) => {
          setOpen(next);
          if (!next) mutation.reset();
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Book with {doctor.name}</DialogTitle>
            <DialogDescription>{doctor.specialty}</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <SlotPicker
              doctorId={doctor.userId}
              date={date}
              time={time}
              onDateChange={setDate}
              onTimeChange={setTime}
            />

            <div className="space-y-2">
              <Label htmlFor="reason">Reason for visit (optional)</Label>
              <Textarea
                id="reason"
                rows={2}
                maxLength={500}
                value={reason}
                onChange={(e) => setReason(e.target.value)}
              />
            </div>

            {mutation.error && <p className="text-sm text-red-500">{mutation.error.message}</p>}
          </div>

          <DialogFooter>
            <Button onClick={() => mutation.mutate()} disabled={!date || !time || mutation.isPending}>
              {mutation.isPending ? "Booking..." : "Confirm booking"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
