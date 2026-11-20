"use client";

import { useQuery } from "@tanstack/react-query";
import { getMyDoctorProfile } from "@/lib/api/doctors";
import DoctorProfileForm from "@/components/shared/DoctorProfileForm";
import PageHeader from "@/components/shared/PageHeader";
import { LoadingState } from "@/components/shared/QueryState";

export default function DoctorProfilePage() {
  // The layout's profile gate guarantees the profile exists
  const { data: profile } = useQuery({ queryKey: ["doctor-profile"], queryFn: getMyDoctorProfile });

  if (!profile) return <LoadingState />;

  return (
    <>
      <PageHeader title="Professional profile" />
      <DoctorProfileForm key={profile.id} profile={profile} />
    </>
  );
}
