"use client";

import { use } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, Droplet, Mail, Phone } from "lucide-react";
import { getPatientForDoctor } from "@/lib/api/patients";
import { getAppointmentsWithPatient } from "@/lib/api/appointments";
import { ApiError } from "@/lib/api/client";
import { dateKey, formatDate, initials, parseDateKey } from "@/lib/format";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import MedicalBackgroundPanel from "@/components/shared/MedicalBackgroundPanel";
import ConsultationHistory from "@/components/shared/ConsultationHistory";
import DocumentsPanel from "@/components/shared/DocumentsPanel";
import DoctorAppointmentCard from "@/components/shared/DoctorAppointmentCard";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

function age(dateOfBirth: string) {
  const birth = parseDateKey(dateKey(dateOfBirth));
  const now = new Date();
  let years = now.getFullYear() - birth.getFullYear();
  const beforeBirthday =
    now.getMonth() < birth.getMonth() ||
    (now.getMonth() === birth.getMonth() && now.getDate() < birth.getDate());
  if (beforeBirthday) years--;
  return years;
}

// The patient file: profile, appointments with this doctor and the three histories
export default function PatientFilePage({ params }: { params: Promise<{ patientId: string }> }) {
  const { patientId } = use(params);

  const { data: patient, isLoading, error } = useQuery({
    queryKey: ["patient-file", patientId],
    queryFn: () => getPatientForDoctor(patientId),
    retry: false,
  });

  const { data: appointments } = useQuery({
    queryKey: ["patient-appointments", patientId],
    queryFn: () => getAppointmentsWithPatient(patientId),
    enabled: !!patient,
  });

  const back = (
    <Link href="/doctor/patients" className={buttonVariants({ variant: "ghost", size: "sm" })}>
      <ArrowLeft />
      All patients
    </Link>
  );

  if (isLoading) return <LoadingState />;
  if (error) {
    return (
      <div className="space-y-4">
        {back}
        <ErrorState
          message={
            error instanceof ApiError && error.status === 404
              ? "This patient hasn't completed their profile yet."
              : error.message
          }
        />
      </div>
    );
  }
  if (!patient) return null;

  return (
    <>
      <div>{back}</div>

      <Card>
        <CardContent className="flex flex-col gap-4 pt-6 sm:flex-row sm:items-center">
          <span className="flex size-14 shrink-0 items-center justify-center rounded-full bg-primary/10 text-lg font-medium text-primary">
            {initials(patient.name)}
          </span>
          <div className="space-y-2">
            <div>
              <h1 className="text-xl font-semibold">{patient.name}</h1>
              <p className="text-sm text-muted-foreground">
                {age(patient.dateOfBirth)} years · born {formatDate(patient.dateOfBirth)}
              </p>
            </div>
            <div className="flex flex-wrap gap-x-5 gap-y-1 text-sm text-muted-foreground">
              <span className="flex items-center gap-1.5"><Mail className="size-3.5" />{patient.email}</span>
              <span className="flex items-center gap-1.5"><Phone className="size-3.5" />{patient.phoneNumber}</span>
              <span className="flex items-center gap-1.5"><Droplet className="size-3.5" />Blood type {patient.bloodType}</span>
            </div>
            {patient.allergies && (
              <p className="text-sm font-medium text-destructive">Allergies: {patient.allergies}</p>
            )}
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="background">
        <TabsList className="flex-wrap">
          <TabsTrigger value="background">Medical history</TabsTrigger>
          <TabsTrigger value="consultations">Consultation history</TabsTrigger>
          <TabsTrigger value="documents">Medical documents</TabsTrigger>
          <TabsTrigger value="appointments">Appointments ({appointments?.length ?? 0})</TabsTrigger>
        </TabsList>

        <TabsContent value="background" className="pt-4">
          <MedicalBackgroundPanel mode="doctor" patientId={patientId} />
        </TabsContent>

        <TabsContent value="consultations" className="pt-4">
          <ConsultationHistory mode="doctor" patientId={patientId} />
        </TabsContent>

        <TabsContent value="documents" className="pt-4">
          <DocumentsPanel mode="doctor" patientId={patientId} />
        </TabsContent>

        <TabsContent value="appointments" className="space-y-3 pt-4">
          {appointments?.length === 0 && <EmptyState title="No appointments" />}
          {appointments?.map((a) => (
            <DoctorAppointmentCard key={a.id} appointment={a} showPatientLink={false} />
          ))}
        </TabsContent>
      </Tabs>
    </>
  );
}
