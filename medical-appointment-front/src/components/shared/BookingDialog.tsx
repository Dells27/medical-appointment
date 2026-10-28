"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { createAppointment } from "@/lib/api/appointments";
import type { Doctor } from "@/types/doctor";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

// Horarios de 09:00 a 16:30, cada 30 minutos
const TIME_SLOTS = Array.from({ length: 16 }, (_, i) => {
  const hour = 9 + Math.floor(i / 2);
  const minutes = i % 2 === 0 ? "00" : "30";
  return `${String(hour).padStart(2, "0")}:${minutes}`;
});

export default function BookingDialog({ doctor }: { doctor: Doctor }) {
  const token = useAppSelector((state) => state.auth.token);
  const queryClient = useQueryClient();

  const [open, setOpen] = useState(false);
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");
  const [reason, setReason] = useState("");

  const today = new Date().toLocaleDateString("en-CA"); // YYYY-MM-DD en hora local

  const mutation = useMutation({
    mutationFn: () =>
      createAppointment(token!, {
        doctorId: doctor.userId, // el UserId del médico, no su Id
        appointmentDate: date,
        appointmentTime: `${time}:00`,
        reason: reason || undefined,
      }),
    onSuccess: () => {
      toast.success("Appointment booked");
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      setOpen(false);
      setDate("");
      setTime("");
      setReason("");
    },
  });

  return (
    <>
      <Button className="w-full" onClick={() => setOpen(true)}>
        Book appointment
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Book with {doctor.name}</DialogTitle>
            <DialogDescription>{doctor.specialty}</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="date">Date</Label>
              <Input
                id="date"
                type="date"
                min={today}
                value={date}
                onChange={(e) => setDate(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>Time</Label>
              <div className="grid grid-cols-4 gap-2">
                {TIME_SLOTS.map((slot) => (
                  <Button
                    key={slot}
                    type="button"
                    size="sm"
                    variant={time === slot ? "default" : "outline"}
                    onClick={() => setTime(slot)}
                  >
                    {slot}
                  </Button>
                ))}
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="reason">Reason for visit (optional)</Label>
              <Input
                id="reason"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
              />
            </div>

            {mutation.error && (
              <p className="text-sm text-red-500">{mutation.error.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button
              onClick={() => mutation.mutate()}
              disabled={!date || !time || mutation.isPending}
            >
              {mutation.isPending ? "Booking..." : "Confirm"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}