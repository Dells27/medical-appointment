"use client";

import Link from "next/link";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Check, CheckCheck, FolderOpen, RefreshCw } from "lucide-react";
import { completeAppointment, confirmAppointment } from "@/lib/api/appointments";
import { isActiveStatus } from "@/lib/appointment-status";
import { dateKey, formatDate, formatTime, initials, todayKey } from "@/lib/format";
import type { Appointment } from "@/types/appointment";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import StatusBadge from "./StatusBadge";
import CancelAppointmentDialog from "./CancelAppointmentDialog";
import RescheduleDialog from "./RescheduleDialog";
import NotesDialog from "./NotesDialog";

// One appointment with every action available to the doctor
export default function DoctorAppointmentCard({
  appointment,
  showPatientLink = true,
}: {
  appointment: Appointment;
  showPatientLink?: boolean;
}) {
  const queryClient = useQueryClient();

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["schedule"] });
    queryClient.invalidateQueries({ queryKey: ["doctor-stats"] });
    queryClient.invalidateQueries({ queryKey: ["patient-appointments", appointment.patientId] });
  };

  const confirm = useMutation({
    mutationFn: () => confirmAppointment(appointment.id),
    onSuccess: () => {
      toast.success("Appointment confirmed. The patient was notified.");
      refresh();
    },
    onError: (err) => toast.error(err.message),
  });

  const complete = useMutation({
    mutationFn: () => completeAppointment(appointment.id),
    onSuccess: () => {
      toast.success("Appointment completed. You can now write the clinical notes.");
      refresh();
      queryClient.invalidateQueries({ queryKey: ["consultations"] });
    },
    onError: (err) => toast.error(err.message),
  });

  const active = isActiveStatus(appointment.status);
  // A visit can only be completed on or after its day
  const canComplete = active && dateKey(appointment.appointmentDate) <= todayKey();

  return (
    <Card>
      <CardContent className="flex flex-col gap-4 pt-6 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex gap-4">
          <span className="flex size-11 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-medium text-primary">
            {initials(appointment.patientName)}
          </span>
          <div className="space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <p className="font-medium">{appointment.patientName ?? "Patient"}</p>
              <StatusBadge status={appointment.status} />
            </div>
            <p className="text-sm text-muted-foreground">
              {formatDate(appointment.appointmentDate)} at {formatTime(appointment.appointmentTime)}
            </p>
            {appointment.reason && <p className="text-sm">Reason: {appointment.reason}</p>}
            {appointment.rescheduledAt && active && (
              <p className="flex items-center gap-1 text-xs text-muted-foreground">
                <RefreshCw className="size-3" /> Rescheduled
              </p>
            )}
            {appointment.status === "Cancelled" && (
              <p className="text-xs text-muted-foreground">
                Cancelled by {appointment.cancelledByRole === "Doctor" ? "you" : "the patient"}
                {appointment.cancellationReason ? `: ${appointment.cancellationReason}` : ""}
              </p>
            )}
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {appointment.status === "Scheduled" && (
            <Button size="sm" onClick={() => confirm.mutate()} disabled={confirm.isPending}>
              <Check />
              Confirm
            </Button>
          )}
          {canComplete && (
            <Button size="sm" variant="secondary" onClick={() => complete.mutate()} disabled={complete.isPending}>
              <CheckCheck />
              Mark as completed
            </Button>
          )}
          {active && <RescheduleDialog appointment={appointment} />}
          {active && <CancelAppointmentDialog appointment={appointment} />}
          {appointment.status === "Completed" && <NotesDialog appointment={appointment} />}
          {showPatientLink && (
            <Link
              href={`/doctor/patients/${appointment.patientId}`}
              className={buttonVariants({ variant: "ghost", size: "sm" })}
            >
              <FolderOpen />
              Patient file
            </Link>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
