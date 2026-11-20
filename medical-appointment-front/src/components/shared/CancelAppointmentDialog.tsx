"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { cancelAppointment } from "@/lib/api/appointments";
import { formatDate, formatTime } from "@/lib/format";
import type { Appointment } from "@/types/appointment";
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

// Both patients and doctors cancel through this dialog; the other party gets an email
export default function CancelAppointmentDialog({
  appointment,
  size = "sm",
}: {
  appointment: Appointment;
  size?: "sm" | "default";
}) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [reason, setReason] = useState("");

  const mutation = useMutation({
    mutationFn: () => cancelAppointment(appointment.id, reason.trim()),
    onSuccess: () => {
      toast.success("Appointment cancelled");
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      queryClient.invalidateQueries({ queryKey: ["schedule"] });
      queryClient.invalidateQueries({ queryKey: ["doctor-stats"] });
      setOpen(false);
      setReason("");
    },
  });

  return (
    <>
      <Button variant="outline" size={size} onClick={() => setOpen(true)}>
        Cancel
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel appointment?</DialogTitle>
            <DialogDescription>
              {formatDate(appointment.appointmentDate)} at {formatTime(appointment.appointmentTime)}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-2">
            <Label htmlFor="cancel-reason">Reason (optional)</Label>
            <Textarea
              id="cancel-reason"
              rows={3}
              maxLength={500}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>

          {mutation.error && <p className="text-sm text-red-500">{mutation.error.message}</p>}

          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>
              Keep appointment
            </Button>
            <Button variant="destructive" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
              {mutation.isPending ? "Cancelling..." : "Cancel appointment"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
