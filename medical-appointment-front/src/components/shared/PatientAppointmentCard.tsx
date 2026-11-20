import { CalendarClock, RefreshCw } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { formatDate, formatTime } from "@/lib/format";
import { isActiveStatus } from "@/lib/appointment-status";
import type { Appointment } from "@/types/appointment";
import type { PublicDoctor } from "@/types/doctor";
import StatusBadge from "./StatusBadge";
import CancelAppointmentDialog from "./CancelAppointmentDialog";

export default function PatientAppointmentCard({
  appointment,
  doctor,
  allowCancel = true,
}: {
  appointment: Appointment;
  doctor?: PublicDoctor;
  allowCancel?: boolean;
}) {
  return (
    <Card>
      <CardContent className="flex flex-col gap-4 pt-6 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex gap-4">
          <div className="flex size-12 shrink-0 flex-col items-center justify-center rounded-lg bg-primary/10 text-primary">
            <CalendarClock className="size-5" />
          </div>
          <div className="space-y-1">
            <p className="font-medium">{doctor ? `Dr. ${doctor.name}` : "Doctor"}</p>
            <p className="text-sm text-muted-foreground">
              {doctor?.specialty && `${doctor.specialty} · `}
              {formatDate(appointment.appointmentDate)} at {formatTime(appointment.appointmentTime)}
            </p>
            {appointment.reason && <p className="text-sm">{appointment.reason}</p>}
            {appointment.rescheduledAt && isActiveStatus(appointment.status) && (
              <p className="flex items-center gap-1 text-xs text-amber-700 dark:text-amber-400">
                <RefreshCw className="size-3" /> Rescheduled by your doctor
              </p>
            )}
            {appointment.status === "Cancelled" && appointment.cancellationReason && (
              <p className="text-xs text-muted-foreground">
                Cancelled by {appointment.cancelledByRole === "Doctor" ? "your doctor" : "you"}:{" "}
                {appointment.cancellationReason}
              </p>
            )}
          </div>
        </div>

        <div className="flex items-center gap-3">
          <StatusBadge status={appointment.status} />
          {allowCancel && isActiveStatus(appointment.status) && (
            <CancelAppointmentDialog appointment={appointment} />
          )}
        </div>
      </CardContent>
    </Card>
  );
}
