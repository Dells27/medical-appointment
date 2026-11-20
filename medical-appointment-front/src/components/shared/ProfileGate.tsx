"use client";

import { useQuery } from "@tanstack/react-query";
import { ApiError } from "@/lib/api/client";
import { getPatientProfile } from "@/lib/api/patients";
import { getMyDoctorProfile } from "@/lib/api/doctors";
import PatientProfileForm from "./PatientProfileForm";
import DoctorProfileForm from "./DoctorProfileForm";
import { ErrorState, LoadingState } from "./QueryState";

// Until the user completes their profile, every page in their area shows the profile form.
// A 404 from the profile endpoint simply means "no profile yet".

export function PatientProfileGate({ children }: { children: React.ReactNode }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["patient-profile"],
    queryFn: getPatientProfile,
    retry: false,
  });

  if (isLoading) return <LoadingState />;
  if (error instanceof ApiError && error.status === 404) return <PatientProfileForm />;
  if (error) return <ErrorState message="Could not load your profile." />;
  if (!data) return <LoadingState />;

  return <>{children}</>;
}

export function DoctorProfileGate({ children }: { children: React.ReactNode }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["doctor-profile"],
    queryFn: getMyDoctorProfile,
    retry: false,
  });

  if (isLoading) return <LoadingState />;
  if (error instanceof ApiError && error.status === 404) return <DoctorProfileForm />;
  if (error) return <ErrorState message="Could not load your profile." />;
  if (!data) return <LoadingState />;

  return <>{children}</>;
}
