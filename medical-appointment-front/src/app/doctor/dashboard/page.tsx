"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { CalendarCheck, CalendarClock, Hourglass, Users, XCircle } from "lucide-react";
import { useAppSelector } from "@/store/hooks";
import { getMySchedule, getMyStats } from "@/lib/api/appointments";
import { isActiveStatus } from "@/lib/appointment-status";
import { dateKey, todayKey } from "@/lib/format";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import PageHeader from "@/components/shared/PageHeader";
import DoctorAppointmentCard from "@/components/shared/DoctorAppointmentCard";
import CompletedVisitsChart from "@/components/shared/CompletedVisitsChart";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

function StatTile({
  label,
  value,
  icon: Icon,
  hint,
}: {
  label: string;
  value: string | number;
  icon: React.ComponentType<{ className?: string }>;
  hint?: string;
}) {
  return (
    <Card>
      <CardContent className="space-y-1 pt-6">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          {label}
          <Icon className="size-4" />
        </div>
        <p className="text-3xl font-semibold tabular-nums">{value}</p>
        {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
      </CardContent>
    </Card>
  );
}

export default function DoctorDashboard() {
  const user = useAppSelector((state) => state.auth.user);

  const { data: stats, isLoading: statsLoading, error: statsError } = useQuery({
    queryKey: ["doctor-stats"],
    queryFn: getMyStats,
  });

  const { data: appointments, isLoading } = useQuery({
    queryKey: ["schedule"],
    queryFn: getMySchedule,
  });

  const today = todayKey();
  const byTime = (a: { appointmentTime: string }, b: { appointmentTime: string }) =>
    a.appointmentTime.localeCompare(b.appointmentTime);

  const todays = (appointments ?? [])
    .filter((a) => isActiveStatus(a.status) && dateKey(a.appointmentDate) === today)
    .sort(byTime);

  const pending = (appointments ?? [])
    .filter((a) => a.status === "Scheduled" && dateKey(a.appointmentDate) >= today)
    .sort((a, b) =>
      (dateKey(a.appointmentDate) + a.appointmentTime).localeCompare(dateKey(b.appointmentDate) + b.appointmentTime),
    );

  return (
    <>
      <PageHeader
        title={`Good to see you, Dr. ${user?.name?.split(" ").at(-1) ?? ""}`}
        description="Your agenda and care statistics at a glance."
      />

      {statsLoading && <LoadingState />}
      {statsError && <ErrorState message={statsError.message} />}

      {stats && (
        <div className="grid grid-cols-2 gap-3 lg:grid-cols-5">
          <StatTile label="Today" value={stats.today} icon={CalendarClock} hint="appointments" />
          <StatTile label="Pending confirmation" value={stats.pendingConfirmation} icon={Hourglass} />
          <StatTile label="Upcoming" value={stats.upcoming} icon={CalendarCheck} hint="scheduled or confirmed" />
          <StatTile label="Patients attended" value={stats.patientsAttended} icon={Users} hint="all time" />
          <StatTile
            label="Cancellation rate"
            value={`${Math.round(stats.cancellationRate * 100)}%`}
            icon={XCircle}
            hint={`${stats.byStatus.Cancelled} of ${stats.totalAppointments}`}
          />
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-5">
        <Card className="lg:col-span-3">
          <CardHeader>
            <CardTitle>Completed visits</CardTitle>
            <CardDescription>Last six months</CardDescription>
          </CardHeader>
          <CardContent>
            {stats && <CompletedVisitsChart data={stats.lastSixMonths} />}
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <div className="space-y-1">
              <CardTitle>Today</CardTitle>
              <CardDescription>{todays.length} active appointments</CardDescription>
            </div>
            <Link href="/doctor/agenda" className={buttonVariants({ variant: "link", size: "sm" })}>
              Open agenda
            </Link>
          </CardHeader>
          <CardContent className="space-y-2">
            {isLoading && <LoadingState />}
            {appointments && todays.length === 0 && (
              <p className="text-sm text-muted-foreground">No appointments today.</p>
            )}
            {todays.map((a) => (
              <Link
                key={a.id}
                href={`/doctor/patients/${a.patientId}`}
                className="flex items-center justify-between rounded-md border px-3 py-2 text-sm hover:bg-muted"
              >
                <span className="font-medium">{a.patientName ?? "Patient"}</span>
                <span className="tabular-nums text-muted-foreground">{a.appointmentTime.slice(0, 5)}</span>
              </Link>
            ))}
          </CardContent>
        </Card>
      </div>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Waiting for your confirmation</h2>
          <Link href="/doctor/appointments" className={buttonVariants({ variant: "link", size: "sm" })}>
            All appointments
          </Link>
        </div>
        {appointments && pending.length === 0 && (
          <EmptyState icon={CalendarCheck} title="You're all caught up" description="No appointments waiting for confirmation." />
        )}
        {pending.slice(0, 5).map((a) => (
          <DoctorAppointmentCard key={a.id} appointment={a} />
        ))}
      </section>
    </>
  );
}
