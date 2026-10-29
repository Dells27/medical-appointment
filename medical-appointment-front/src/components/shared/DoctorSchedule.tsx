"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { completeAppointment, getMySchedule } from "@/lib/api/appointments";
import { statusVariant } from "@/lib/appointment-status";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

export default function DoctorSchedule() {
  const token = useAppSelector((state) => state.auth.token);
  const queryClient = useQueryClient();

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ["schedule"],
    queryFn: () => getMySchedule(token!),
    enabled: !!token,
  });

  const complete = useMutation({
    mutationFn: (id: string) => completeAppointment(token!, id),
    onSuccess: () => {
      toast.success("Appointment completed");
      queryClient.invalidateQueries({ queryKey: ["schedule"] });
    },
    onError: (err) => toast.error(err.message),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">My schedule</h1>

      {isLoading && <p>Loading schedule...</p>}
      {error && <p className="text-red-500">Could not load your schedule.</p>}
      {appointments?.length === 0 && <p>You have no appointments yet.</p>}

      <div className="space-y-3">
        {appointments?.map((appointment) => {
          const canComplete =
            appointment.status === "Scheduled" || appointment.status === "Confirmed";

          return (
            <Card key={appointment.id}>
              <CardContent className="flex items-center justify-between gap-4 pt-6">
                <div className="space-y-1">
                  <p className="font-medium">
                    {appointment.appointmentDate.slice(0, 10)} at{" "}
                    {appointment.appointmentTime.slice(0, 5)}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Patient #{appointment.patientId.slice(0, 8)}
                  </p>
                  {appointment.reason && <p className="text-sm">{appointment.reason}</p>}
                </div>

                <div className="flex items-center gap-3">
                  <Badge variant={statusVariant(appointment.status)}>{appointment.status}</Badge>
                  {canComplete && (
                    <Button
                      size="sm"
                      disabled={complete.isPending}
                      onClick={() => complete.mutate(appointment.id)}
                    >
                      Mark as completed
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