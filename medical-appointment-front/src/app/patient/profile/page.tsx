"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Pencil } from "lucide-react";
import { getPatientProfile } from "@/lib/api/patients";
import { formatDate } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import PageHeader from "@/components/shared/PageHeader";
import PatientProfileForm from "@/components/shared/PatientProfileForm";
import { LoadingState } from "@/components/shared/QueryState";

export default function PatientProfilePage() {
  const [editing, setEditing] = useState(false);

  // The layout's profile gate guarantees the profile exists
  const { data: profile } = useQuery({ queryKey: ["patient-profile"], queryFn: getPatientProfile });

  if (!profile) return <LoadingState />;

  return (
    <>
      <PageHeader title="My profile" description="Your personal information." />

      {editing ? (
        <PatientProfileForm profile={profile} onDone={() => setEditing(false)} />
      ) : (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>{profile.name}</CardTitle>
            <Button size="sm" variant="outline" onClick={() => setEditing(true)}>
              <Pencil />
              Edit
            </Button>
          </CardHeader>
          <CardContent>
            <dl className="grid gap-4 text-sm sm:grid-cols-2">
              <Field label="Email" value={profile.email} />
              <Field label="Phone" value={profile.phoneNumber} />
              <Field label="Date of birth" value={formatDate(profile.dateOfBirth)} />
              <Field label="Blood type" value={profile.bloodType} />
              <Field label="Known allergies" value={profile.allergies || "None reported"} wide />
            </dl>
          </CardContent>
        </Card>
      )}
    </>
  );
}

function Field({ label, value, wide }: { label: string; value: string; wide?: boolean }) {
  return (
    <div className={wide ? "sm:col-span-2" : ""}>
      <dt className="font-medium">{label}</dt>
      <dd className="text-muted-foreground">{value}</dd>
    </div>
  );
}
