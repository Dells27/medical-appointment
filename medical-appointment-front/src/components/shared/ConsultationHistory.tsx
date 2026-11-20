"use client";

import { useQuery } from "@tanstack/react-query";
import { NotebookPen } from "lucide-react";
import { getMyHistory, getPatientHistory } from "@/lib/api/medical-record";
import { getPublicDoctors } from "@/lib/api/doctors";
import { formatDate } from "@/lib/format";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { EmptyState, ErrorState, LoadingState } from "./QueryState";

// Past consultations with diagnosis and treatment.
// Patient mode: the patient's own history. Doctor mode: a patient's history with any doctor.
export default function ConsultationHistory(
  props: { mode: "patient" } | { mode: "doctor"; patientId: string },
) {
  const isDoctor = props.mode === "doctor";
  const patientId = isDoctor ? props.patientId : undefined;

  const { data: records, isLoading, error } = useQuery({
    queryKey: isDoctor ? ["patient-history", patientId] : ["history"],
    queryFn: () => (isDoctor ? getPatientHistory(patientId!) : getMyHistory()),
  });

  // Doctor names come from the public directory (records only store the doctor's user id)
  const { data: doctors } = useQuery({
    queryKey: ["public-doctors", "", ""],
    queryFn: () => getPublicDoctors(),
  });

  function doctorLabel(userId: string) {
    const doctor = doctors?.find((d) => d.userId === userId);
    return doctor ? `Dr. ${doctor.name} · ${doctor.specialty}` : "Doctor";
  }

  if (isLoading) return <LoadingState label="Loading consultations..." />;
  if (error) return <ErrorState message={error.message} />;
  if (!records?.length) {
    return (
      <EmptyState
        icon={NotebookPen}
        title="No consultations yet"
        description="Completed appointments and their clinical notes will appear here."
      />
    );
  }

  return (
    <ol className="relative space-y-4 border-l pl-6">
      {records.map((record) => (
        <li key={record.id} className="relative">
          <span className="absolute top-6 -left-[29px] size-2.5 rounded-full border-2 border-background bg-primary" />
          <Card>
            <CardContent className="space-y-3 pt-6">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-medium">{formatDate(record.appointmentDate)}</p>
                  <p className="text-sm text-muted-foreground">{doctorLabel(record.doctorId)}</p>
                </div>
                {!record.isCompleted && <Badge variant="outline">Notes pending</Badge>}
              </div>

              {record.isCompleted && (
                <dl className="grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="font-medium">Diagnosis</dt>
                    <dd className="whitespace-pre-wrap text-muted-foreground">{record.diagnosis}</dd>
                  </div>
                  <div>
                    <dt className="font-medium">Treatment & prescription</dt>
                    <dd className="whitespace-pre-wrap text-muted-foreground">{record.treatment}</dd>
                  </div>
                  {record.observations && (
                    <div className="sm:col-span-2">
                      <dt className="font-medium">Observations</dt>
                      <dd className="whitespace-pre-wrap text-muted-foreground">{record.observations}</dd>
                    </div>
                  )}
                </dl>
              )}
            </CardContent>
          </Card>
        </li>
      ))}
    </ol>
  );
}
