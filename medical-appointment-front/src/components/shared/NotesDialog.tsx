"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { fillNotes, getRecordByAppointment } from "@/lib/api/medical-record";
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

export default function NotesDialog({ appointment }: { appointment: Appointment }) {
  const token = useAppSelector((state) => state.auth.token);
  const queryClient = useQueryClient();

  const [open, setOpen] = useState(false);
  const [diagnosis, setDiagnosis] = useState("");
  const [treatment, setTreatment] = useState("");
  const [observations, setObservations] = useState("");

  const { data: record, isLoading, error, refetch } = useQuery({
    queryKey: ["record", appointment.id],
    queryFn: () => getRecordByAppointment(token!, appointment.id),
    enabled: open && !!token, // solo pedimos el registro al abrir el diálogo
    retry: false,
  });

  const save = useMutation({
    mutationFn: () =>
      fillNotes(token!, record!.id, {
        diagnosis,
        treatment,
        observations: observations || undefined,
      }),
    onSuccess: () => {
      toast.success("Clinical notes saved");
      queryClient.invalidateQueries({ queryKey: ["record", appointment.id] });
    },
  });

  return (
    <>
      <Button size="sm" variant="outline" onClick={() => setOpen(true)}>
        Clinical notes
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Clinical notes</DialogTitle>
            <DialogDescription>
              Visit on {appointment.appointmentDate.slice(0, 10)}
            </DialogDescription>
          </DialogHeader>

          {isLoading && <p>Loading...</p>}

          {error && (
            <div className="space-y-2">
              <p className="text-sm text-muted-foreground">
                The record is still being prepared. Try again in a moment.
              </p>
              <Button size="sm" variant="outline" onClick={() => refetch()}>
                Try again
              </Button>
            </div>
          )}

          {record?.isCompleted && (
            <div className="space-y-3 text-sm">
              <div>
                <p className="font-medium">Diagnosis</p>
                <p>{record.diagnosis}</p>
              </div>
              <div>
                <p className="font-medium">Treatment</p>
                <p>{record.treatment}</p>
              </div>
              {record.observations && (
                <div>
                  <p className="font-medium">Observations</p>
                  <p>{record.observations}</p>
                </div>
              )}
            </div>
          )}

          {record && !record.isCompleted && (
            <>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="diagnosis">Diagnosis</Label>
                  <Textarea id="diagnosis" value={diagnosis} onChange={(e) => setDiagnosis(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="treatment">Treatment</Label>
                  <Textarea id="treatment" value={treatment} onChange={(e) => setTreatment(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="observations">Observations (optional)</Label>
                  <Textarea id="observations" value={observations} onChange={(e) => setObservations(e.target.value)} />
                </div>
                {save.error && <p className="text-sm text-red-500">{save.error.message}</p>}
              </div>

              <DialogFooter>
                <Button
                  onClick={() => save.mutate()}
                  disabled={!diagnosis.trim() || !treatment.trim() || save.isPending}
                >
                  {save.isPending ? "Saving..." : "Save notes"}
                </Button>
              </DialogFooter>
            </>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}