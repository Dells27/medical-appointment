"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { CalendarX, Search } from "lucide-react";
import { getMySchedule } from "@/lib/api/appointments";
import { isActiveStatus } from "@/lib/appointment-status";
import { dateKey, todayKey } from "@/lib/format";
import type { Appointment } from "@/types/appointment";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import PageHeader from "@/components/shared/PageHeader";
import DoctorAppointmentCard from "@/components/shared/DoctorAppointmentCard";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

const sortKey = (a: Appointment) => dateKey(a.appointmentDate) + a.appointmentTime;

export default function DoctorAppointmentsPage() {
  const [search, setSearch] = useState("");

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ["schedule"],
    queryFn: getMySchedule,
  });

  const today = todayKey();
  const term = search.trim().toLowerCase();
  const filtered = (appointments ?? []).filter(
    (a) => !term || (a.patientName ?? "").toLowerCase().includes(term),
  );

  const asc = (list: Appointment[]) => [...list].sort((a, b) => sortKey(a).localeCompare(sortKey(b)));
  const desc = (list: Appointment[]) => [...list].sort((a, b) => sortKey(b).localeCompare(sortKey(a)));

  const groups = {
    pending: asc(filtered.filter((a) => a.status === "Scheduled" && dateKey(a.appointmentDate) >= today)),
    upcoming: asc(filtered.filter((a) => isActiveStatus(a.status) && dateKey(a.appointmentDate) >= today)),
    // Active appointments whose day has passed still need to be completed (or cancelled)
    past: desc(
      filtered.filter(
        (a) => a.status === "Completed" || a.status === "NoShow" ||
          (isActiveStatus(a.status) && dateKey(a.appointmentDate) < today),
      ),
    ),
    cancelled: desc(filtered.filter((a) => a.status === "Cancelled")),
  };

  const tabs = [
    { value: "pending", label: "Pending confirmation", items: groups.pending },
    { value: "upcoming", label: "Upcoming", items: groups.upcoming },
    { value: "past", label: "Past", items: groups.past },
    { value: "cancelled", label: "Cancelled", items: groups.cancelled },
  ];

  return (
    <>
      <PageHeader
        title="My appointments"
        description="Confirm, reschedule, complete and document your appointments."
      />

      <div className="relative max-w-sm">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          className="pl-9"
          placeholder="Search by patient"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label="Search by patient"
        />
      </div>

      {isLoading && <LoadingState label="Loading appointments..." />}
      {error && <ErrorState message={error.message} />}

      {appointments && (
        <Tabs defaultValue={groups.pending.length > 0 ? "pending" : "upcoming"}>
          <TabsList className="flex-wrap">
            {tabs.map((t) => (
              <TabsTrigger key={t.value} value={t.value}>
                {t.label} ({t.items.length})
              </TabsTrigger>
            ))}
          </TabsList>
          {tabs.map((t) => (
            <TabsContent key={t.value} value={t.value} className="space-y-3 pt-4">
              {t.items.length === 0 && <EmptyState icon={CalendarX} title="No appointments" />}
              {t.items.map((a) => (
                <DoctorAppointmentCard key={a.id} appointment={a} />
              ))}
            </TabsContent>
          ))}
        </Tabs>
      )}
    </>
  );
}
