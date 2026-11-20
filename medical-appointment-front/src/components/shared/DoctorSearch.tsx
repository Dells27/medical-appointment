"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Search, UserSearch } from "lucide-react";
import { getPublicDoctors, getSpecialties } from "@/lib/api/doctors";
import type { PublicDoctor } from "@/types/doctor";
import { Input } from "@/components/ui/input";
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select";
import DoctorCard from "./DoctorCard";
import { EmptyState, ErrorState, LoadingState } from "./QueryState";

// Doctor directory with search and specialty filter. Used on the public site and by patients;
// `renderAction` decides what each card offers (sign in to book, or the booking dialog).
export default function DoctorSearch({
  initialSpecialty = "",
  renderAction,
}: {
  initialSpecialty?: string;
  renderAction: (doctor: PublicDoctor) => React.ReactNode;
}) {
  const [specialty, setSpecialty] = useState(initialSpecialty);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");

  // Wait until the user stops typing before querying
  useEffect(() => {
    const timeout = setTimeout(() => setSearch(searchInput.trim()), 300);
    return () => clearTimeout(timeout);
  }, [searchInput]);

  const { data: specialties } = useQuery({
    queryKey: ["specialties"],
    queryFn: getSpecialties,
  });

  const { data: doctors, isLoading, error } = useQuery({
    queryKey: ["public-doctors", specialty, search],
    queryFn: () => getPublicDoctors({ specialty, search }),
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by name or specialty"
            className="pl-9"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            aria-label="Search doctors"
          />
        </div>
        <NativeSelect
          value={specialty}
          onChange={(e) => setSpecialty(e.target.value)}
          aria-label="Filter by specialty"
          className="sm:w-56"
        >
          <NativeSelectOption value="">All specialties</NativeSelectOption>
          {specialties?.map((s) => (
            <NativeSelectOption key={s.name} value={s.name}>
              {s.name} ({s.doctorCount})
            </NativeSelectOption>
          ))}
        </NativeSelect>
      </div>

      {isLoading && <LoadingState label="Loading doctors..." />}
      {error && <ErrorState message="Could not load doctors." />}
      {doctors?.length === 0 && (
        <EmptyState
          icon={UserSearch}
          title="No doctors found"
          description="Try a different name or specialty."
        />
      )}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {doctors?.map((doctor) => (
          <DoctorCard key={doctor.userId} doctor={doctor} action={renderAction(doctor)} />
        ))}
      </div>
    </div>
  );
}
