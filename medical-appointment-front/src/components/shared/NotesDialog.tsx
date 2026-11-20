"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { NotebookPen } from "lucide-react";
import { fillNotes, getRecordByAppointment } from "@/lib/api/medical-record";
import { formatDate } from "@/lib/format";
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

// Clinical notes for a completed appointment. The record is created by the
// Medical Record Service when it receives the "appointment.completed" event.
export default function NotesDialog({ appointment }: { appointment: Appointment }) {
  const queryClient = useQueryClient();

  const [open, setOpen] = useState(false);
  const [diagnosis, setDiagnosis] = useState("");
  const [treatment, setTreatment] = useState("");
  const [observations, setObservations] = useState("");

  const { data: record, isLoading, error, refetch } = useQuery({
    queryKey: ["record", appointment.id],
    queryFn: () => getRecordByAppointment(appointment.id),
    enabled: open, // only load the record when the dialog opens
    retry: false,
  });

  const save = useMutation({
    mutationFn: () =>
      fillNotes(record!.id, {
        diagnosis: diagnosis.trim(),
        treatment: treatment.trim(),
        observations: observations.trim() || undefined,
      }),
    onSuccess: () => {
      toast.success("Clinical notes saved");
      queryClient.invalidateQueries({ queryKey: ["record", appointment.id] });
      queryClient.invalidateQueries({ queryKey: ["consultations"] });
      queryClient.invalidateQueries({ queryKey: ["patient-history", appointment.patientId] });
    },
  });

  return (
    <>
      <Button size="sm" variant="outline" onClick={() => setOpen(true)}>
        <NotebookPen />
        Clinical notes
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>Clinical notes</DialogTitle>
            <DialogDescription>
              {appointment.patientName ?? "Patient"} · visit on {formatDate(appointment.appointmentDate)}
            </DialogDescription>
          </DialogHeader>

          {isLoading && <p className="text-sm text-muted-foreground">Loading...</p>}

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
                <p className="whitespace-pre-wrap text-muted-foreground">{record.diagnosis}</p>
              </div>
              <div>
                <p className="font-medium">Treatment & prescription</p>
                <p className="whitespace-pre-wrap text-muted-foreground">{record.treatment}</p>
              </div>
              {record.observations && (
                <div>
                  <p className="font-medium">Observations</p>
                  <p className="whitespace-pre-wrap text-muted-foreground">{record.observations}</p>
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
                  <Label htmlFor="treatment">Treatment & prescription</Label>
                  <Textarea
                    id="treatment"
                    placeholder="Medication, dose, frequency and duration"
                    value={treatment}
                    onChange={(e) => setTreatment(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="observations">Observations (optional)</Label>
                  <Textarea id="observations" value={observations} onChange={(e) => setObservations(e.target.value)} />
                </div>
                <p className="text-xs text-muted-foreground">
                  Notes can&apos;t be edited once saved. To share a prescription file, upload it from the
                  patient&apos;s documents.
                </p>
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
