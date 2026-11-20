"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { NotebookPen } from "lucide-react";
import { getMyConsultations } from "@/lib/api/medical-record";
import { getMySchedule } from "@/lib/api/appointments";
import { formatDate } from "@/lib/format";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import PageHeader from "@/components/shared/PageHeader";
import NotesDialog from "@/components/shared/NotesDialog";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

// Every consultation the doctor attended, newest first, with its clinical notes
export default function ConsultationsPage() {
  const { data: records, isLoading, error } = useQuery({
    queryKey: ["consultations"],
    queryFn: getMyConsultations,
  });

  // Appointments carry the patient's name; records only have ids
  const { data: appointments } = useQuery({ queryKey: ["schedule"], queryFn: getMySchedule });
  const appointmentById = new Map((appointments ?? []).map((a) => [a.id, a]));

  const pending = records?.filter((r) => !r.isCompleted).length ?? 0;

  return (
    <>
      <PageHeader
        title="Consultation history"
        description={
          pending > 0
            ? `${pending} consultation${pending === 1 ? "" : "s"} still need clinical notes.`
            : "All the consultations you have attended."
        }
      />

      {isLoading && <LoadingState label="Loading consultations..." />}
      {error && <ErrorState message={error.message} />}
      {records?.length === 0 && (
        <EmptyState
          icon={NotebookPen}
          title="No consultations yet"
          description="Completed appointments appear here so you can document them."
        />
      )}

      <div className="space-y-3">
        {records?.map((record) => {
          const appointment = appointmentById.get(record.appointmentId);
          return (
            <Card key={record.id}>
              <CardContent className="space-y-3 pt-6">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-medium">{appointment?.patientName ?? "Patient"}</p>
                    <p className="text-sm text-muted-foreground">{formatDate(record.appointmentDate)}</p>
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    {record.isCompleted ? (
                      <Badge variant="secondary">Documented</Badge>
                    ) : (
                      <Badge variant="outline">Notes pending</Badge>
                    )}
                    {appointment && <NotesDialog appointment={appointment} />}
                    <Link
                      href={`/doctor/patients/${record.patientId}`}
                      className={buttonVariants({ variant: "ghost", size: "sm" })}
                    >
                      Patient file
                    </Link>
                  </div>
                </div>
                {record.isCompleted && (
                  <p className="line-clamp-2 text-sm text-muted-foreground">
                    <span className="font-medium text-foreground">Diagnosis:</span> {record.diagnosis}
                  </p>
                )}
              </CardContent>
            </Card>
          );
        })}
      </div>
    </>
  );
}
