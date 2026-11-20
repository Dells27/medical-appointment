"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { CalendarCheck, CalendarX, FileText, History, Search } from "lucide-react";
import { useAppSelector } from "@/store/hooks";
import { getMyAppointments } from "@/lib/api/appointments";
import { useDoctorDirectory } from "@/hooks/use-doctor-directory";
import { isActiveStatus } from "@/lib/appointment-status";
import { dateKey, todayKey } from "@/lib/format";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import PageHeader from "@/components/shared/PageHeader";
import PatientAppointmentCard from "@/components/shared/PatientAppointmentCard";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

const QUICK_LINKS = [
  { href: "/patient/doctors", label: "Find a doctor", icon: Search },
  { href: "/patient/appointments", label: "My appointments", icon: CalendarCheck },
  { href: "/patient/history", label: "Medical history", icon: History },
  { href: "/patient/documents", label: "Medical documents", icon: FileText },
];

export default function PatientDashboard() {
  const user = useAppSelector((state) => state.auth.user);
  const findDoctor = useDoctorDirectory();

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ["appointments"],
    queryFn: getMyAppointments,
  });

  const today = todayKey();
  const upcoming = (appointments ?? [])
    .filter((a) => isActiveStatus(a.status) && dateKey(a.appointmentDate) >= today)
    .sort((a, b) =>
      (dateKey(a.appointmentDate) + a.appointmentTime).localeCompare(dateKey(b.appointmentDate) + b.appointmentTime),
    );

  return (
    <>
      <PageHeader
        title={`Hello, ${user?.name?.split(" ")[0] ?? ""}`}
        description="Here is what's coming up for you."
      />

      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        {QUICK_LINKS.map((link) => (
          <Link
            key={link.href}
            href={link.href}
            className="flex items-center gap-3 rounded-lg border bg-card p-4 text-sm font-medium transition-colors hover:border-primary/40 hover:bg-primary/5"
          >
            <link.icon className="size-5 text-primary" />
            {link.label}
          </Link>
        ))}
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Upcoming appointments</CardTitle>
          <Link href="/patient/appointments" className={buttonVariants({ variant: "link", size: "sm" })}>
            View all
          </Link>
        </CardHeader>
        <CardContent className="space-y-3">
          {isLoading && <LoadingState />}
          {error && <ErrorState message={error.message} />}
          {appointments && upcoming.length === 0 && (
            <EmptyState
              icon={CalendarX}
              title="No upcoming appointments"
              description="Find a doctor and book your next visit."
              action={
                <Link href="/patient/doctors" className={buttonVariants({ size: "sm" })}>
                  Find a doctor
                </Link>
              }
            />
          )}
          {upcoming.slice(0, 3).map((a) => (
            <PatientAppointmentCard key={a.id} appointment={a} doctor={findDoctor(a.doctorId)} />
          ))}
        </CardContent>
      </Card>
    </>
  );
}
