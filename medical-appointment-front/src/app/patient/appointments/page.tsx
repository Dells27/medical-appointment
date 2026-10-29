"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { cancelAppointment, getMyAppointments } from "@/lib/api/appointments";
import { getDoctors } from "@/lib/api/doctors";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { statusVariant } from "@/lib/appointment-status";

export default function MyAppointmentsPage() {
  const token = useAppSelector((state) => state.auth.token);
  const queryClient = useQueryClient();

  const {
    data: appointments,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["appointments"],
    queryFn: () => getMyAppointments(token!),
    enabled: !!token,
  });

  // Misma etiqueta que la lista de médicos: reutiliza el cache
  const { data: doctors } = useQuery({
    queryKey: ["doctors"],
    queryFn: () => getDoctors(token!),
    enabled: !!token,
  });

  const cancel = useMutation({
    mutationFn: (id: string) =>
      cancelAppointment(token!, id, "Cancelled by patient"),
    onSuccess: () => {
      toast.success("Appointment cancelled");
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
    },
    onError: (err) => toast.error(err.message),
  });

  // La cita guarda el UserId del médico; lo buscamos en la lista para mostrar su nombre
  function doctorName(userId: string) {
    return doctors?.find((d) => d.userId === userId)?.name ?? "Doctor";
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">My appointments</h1>

      {isLoading && <p>Loading appointments...</p>}
      {error && <p className="text-red-500">Could not load appointments.</p>}
      {appointments?.length === 0 && <p>You have no appointments yet.</p>}

      <div className="space-y-3">
        {appointments?.map((appointment) => {
          const canCancel =
            appointment.status === "Scheduled" ||
            appointment.status === "Confirmed";
            const variant = statusVariant(appointment.status);


          return (
            <Card key={appointment.id}>
              <CardContent className="flex items-center justify-between gap-4 pt-6">
                <div className="space-y-1">
                  <p className="font-medium">
                    {doctorName(appointment.doctorId)}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {appointment.appointmentDate.slice(0, 10)} at{" "}
                    {appointment.appointmentTime.slice(0, 5)}
                  </p>
                  {appointment.reason && (
                    <p className="text-sm">{appointment.reason}</p>
                  )}
                </div>

                <div className="flex items-center gap-3">
                  <Badge variant={variant}>{appointment.status}</Badge>
                  {canCancel && (
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={cancel.isPending}
                      onClick={() => {
                        if (window.confirm("Cancel this appointment?")) {
                          cancel.mutate(appointment.id);
                        }
                      }}
                    >
                      Cancel
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
