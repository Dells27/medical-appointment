"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { CalendarX } from "lucide-react";
import { getMyAppointments } from "@/lib/api/appointments";
import { useDoctorDirectory } from "@/hooks/use-doctor-directory";
import { isActiveStatus } from "@/lib/appointment-status";
import { dateKey, todayKey } from "@/lib/format";
import type { Appointment } from "@/types/appointment";
import { buttonVariants } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import PageHeader from "@/components/shared/PageHeader";
import PatientAppointmentCard from "@/components/shared/PatientAppointmentCard";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

const sortKey = (a: Appointment) => dateKey(a.appointmentDate) + a.appointmentTime;

export default function MyAppointmentsPage() {
  const findDoctor = useDoctorDirectory();

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ["appointments"],
    queryFn: getMyAppointments,
  });

  const today = todayKey();
  const upcoming = (appointments ?? [])
    .filter((a) => isActiveStatus(a.status) && dateKey(a.appointmentDate) >= today)
    .sort((a, b) => sortKey(a).localeCompare(sortKey(b)));
  const past = (appointments ?? [])
    .filter((a) => !upcoming.includes(a))
    .sort((a, b) => sortKey(b).localeCompare(sortKey(a)));

  return (
    <>
      <PageHeader
        title="My appointments"
        actions={
          <Link href="/patient/doctors" className={buttonVariants()}>
            Book an appointment
          </Link>
        }
      />

      {isLoading && <LoadingState label="Loading appointments..." />}
      {error && <ErrorState message={error.message} />}

      {appointments && (
        <Tabs defaultValue="upcoming">
          <TabsList>
            <TabsTrigger value="upcoming">Upcoming ({upcoming.length})</TabsTrigger>
            <TabsTrigger value="past">Past & cancelled ({past.length})</TabsTrigger>
          </TabsList>

          <TabsContent value="upcoming" className="space-y-3 pt-4">
            {upcoming.length === 0 && (
              <EmptyState icon={CalendarX} title="No upcoming appointments" />
            )}
            {upcoming.map((a) => (
              <PatientAppointmentCard key={a.id} appointment={a} doctor={findDoctor(a.doctorId)} />
            ))}
          </TabsContent>

          <TabsContent value="past" className="space-y-3 pt-4">
            {past.length === 0 && <EmptyState icon={CalendarX} title="Nothing here yet" />}
            {past.map((a) => (
              <PatientAppointmentCard
                key={a.id}
                appointment={a}
                doctor={findDoctor(a.doctorId)}
                allowCancel={false}
              />
            ))}
          </TabsContent>
        </Tabs>
      )}
    </>
  );
}
