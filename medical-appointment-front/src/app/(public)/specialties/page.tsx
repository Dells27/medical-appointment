"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { ChevronRight, HeartPulse } from "lucide-react";
import { getSpecialties } from "@/lib/api/doctors";
import PageHeader from "@/components/shared/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/components/shared/QueryState";

export default function SpecialtiesPage() {
  const { data: specialties, isLoading, error } = useQuery({
    queryKey: ["specialties"],
    queryFn: getSpecialties,
  });

  return (
    <div className="mx-auto max-w-6xl space-y-8 px-4 py-12">
      <PageHeader
        title="Specialties"
        description="Choose a specialty to see the doctors who practice it."
      />

      {isLoading && <LoadingState />}
      {error && <ErrorState message="Could not load specialties." />}
      {specialties?.length === 0 && (
        <EmptyState icon={HeartPulse} title="No specialties yet" />
      )}

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {specialties?.map((s) => (
          <Link
            key={s.name}
            href={`/doctors?specialty=${encodeURIComponent(s.name)}`}
            className="group flex items-center justify-between rounded-lg border bg-card p-4 transition-colors hover:border-primary/40 hover:bg-primary/5"
          >
            <div className="flex items-center gap-3">
              <HeartPulse className="size-5 text-primary" />
              <div>
                <p className="font-medium">{s.name}</p>
                <p className="text-xs text-muted-foreground">
                  {s.doctorCount} {s.doctorCount === 1 ? "doctor" : "doctors"}
                </p>
              </div>
            </div>
            <ChevronRight className="size-4 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
          </Link>
        ))}
      </div>
    </div>
  );
}
