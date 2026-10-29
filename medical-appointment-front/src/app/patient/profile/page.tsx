"use client";

import { useQuery } from "@tanstack/react-query";
import { useAppSelector } from "@/store/hooks";
import { getPatientProfile } from "@/lib/api/patients";
import { ApiError } from "@/lib/api/client";
import PatientProfileForm from "@/components/shared/PatientProfileForm";
import DocumentsSection from "@/components/shared/DocumentsSection";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function PatientProfilePage() {
  const token = useAppSelector((state) => state.auth.token);

  const { data: profile, isLoading, error } = useQuery({
    queryKey: ["patient-profile"],
    queryFn: () => getPatientProfile(token!),
    enabled: !!token,
    retry: false, // un 404 es una respuesta válida: "todavía no hay perfil"
  });

  if (isLoading) return <p>Loading...</p>;

  if (error instanceof ApiError && error.status === 404) {
    return <PatientProfileForm />;
  }

  if (error) return <p className="text-red-500">Could not load your profile.</p>;
  if (!profile) return <p>Loading...</p>;

  return (
    <div className="space-y-8">
      <h1 className="text-2xl font-semibold">My profile</h1>

      <Card>
        <CardHeader>
          <CardTitle>{profile.fullName}</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-2 text-sm sm:grid-cols-2">
          <p><span className="font-medium">Email:</span> {profile.email}</p>
          <p><span className="font-medium">Phone:</span> {profile.phoneNumber}</p>
          <p><span className="font-medium">Date of birth:</span> {profile.dateOfBirth.slice(0, 10)}</p>
          <p><span className="font-medium">Blood type:</span> {profile.bloodType}</p>
          <p className="sm:col-span-2">
            <span className="font-medium">Allergies:</span> {profile.allergies || "None"}
          </p>
        </CardContent>
      </Card>

      <DocumentsSection />
    </div>
  );
}