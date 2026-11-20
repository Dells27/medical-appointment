"use client";

import { useQuery } from "@tanstack/react-query";
import { getPublicDoctors } from "@/lib/api/doctors";

// Appointments and records store the doctor's user id; this resolves it to name/specialty.
// Shares the cache with the unfiltered doctor directory.
export function useDoctorDirectory() {
  const { data: doctors } = useQuery({
    queryKey: ["public-doctors", "", ""],
    queryFn: () => getPublicDoctors(),
    staleTime: 5 * 60 * 1000,
  });

  return function findDoctor(userId: string) {
    return doctors?.find((d) => d.userId === userId);
  };
}
