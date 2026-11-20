"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { ChevronRight, Search, Users } from "lucide-react";
import { getMyPatients } from "@/lib/api/appointments";
import { formatDate, formatTime, initials } from "@/lib/format";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import PageHeader from "@/components/shared/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

export default function DoctorPatientsPage() {
  const [search, setSearch] = useState("");

  const { data: patients, isLoading, error } = useQuery({
    queryKey: ["my-patients"],
    queryFn: getMyPatients,
  });

  const term = search.trim().toLowerCase();
  const visible = (patients ?? []).filter(
    (p) =>
      !term ||
      (p.patientName ?? "").toLowerCase().includes(term) ||
      (p.patientEmail ?? "").toLowerCase().includes(term),
  );

  return (
    <>
      <PageHeader
        title="Patients"
        description="Everyone who has booked with you. Open a patient to see their history and documents."
      />

      <div className="relative max-w-sm">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          className="pl-9"
          placeholder="Search by name or email"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label="Search patients"
        />
      </div>

      {isLoading && <LoadingState label="Loading patients..." />}
      {error && <ErrorState message={error.message} />}
      {patients && visible.length === 0 && (
        <EmptyState icon={Users} title="No patients yet" description="Patients appear here once they book with you." />
      )}

      {visible.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Patient</TableHead>
                <TableHead className="text-right">Visits</TableHead>
                <TableHead>Last visit</TableHead>
                <TableHead>Next appointment</TableHead>
                <TableHead className="w-8" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {visible.map((p) => (
                <TableRow key={p.patientId} className="cursor-pointer">
                  <TableCell>
                    <Link href={`/doctor/patients/${p.patientId}`} className="flex items-center gap-3">
                      <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-medium text-primary">
                        {initials(p.patientName)}
                      </span>
                      <span className="min-w-0">
                        <span className="block truncate font-medium">{p.patientName ?? "Patient"}</span>
                        <span className="block truncate text-xs text-muted-foreground">{p.patientEmail}</span>
                      </span>
                    </Link>
                  </TableCell>
                  <TableCell className="text-right tabular-nums">{p.completedVisits}</TableCell>
                  <TableCell>{p.lastVisit ? formatDate(p.lastVisit) : "—"}</TableCell>
                  <TableCell>
                    {p.nextAppointmentDate
                      ? `${formatDate(p.nextAppointmentDate)} · ${formatTime(p.nextAppointmentTime!)}`
                      : "—"}
                  </TableCell>
                  <TableCell>
                    <Link href={`/doctor/patients/${p.patientId}`} aria-label={`Open ${p.patientName ?? "patient"}`}>
                      <ChevronRight className="size-4 text-muted-foreground" />
                    </Link>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </>
  );
}
