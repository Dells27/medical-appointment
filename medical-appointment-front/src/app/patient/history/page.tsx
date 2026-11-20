"use client";

import { useQuery } from "@tanstack/react-query";
import { getPatientProfile } from "@/lib/api/patients";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import PageHeader from "@/components/shared/PageHeader";
import MedicalBackgroundPanel from "@/components/shared/MedicalBackgroundPanel";
import ConsultationHistory from "@/components/shared/ConsultationHistory";

export default function MedicalHistoryPage() {
  // Already loaded by the profile gate — used to show the allergies the patient reported
  const { data: profile } = useQuery({ queryKey: ["patient-profile"], queryFn: getPatientProfile });

  return (
    <>
      <PageHeader
        title="Medical history"
        description="Your medical background and the notes from each consultation."
      />

      <Tabs defaultValue="background">
        <TabsList>
          <TabsTrigger value="background">Medical history</TabsTrigger>
          <TabsTrigger value="consultations">Consultation history</TabsTrigger>
        </TabsList>
        <TabsContent value="background" className="pt-4">
          <MedicalBackgroundPanel mode="patient" patientAllergies={profile?.allergies} />
        </TabsContent>
        <TabsContent value="consultations" className="pt-4">
          <ConsultationHistory mode="patient" />
        </TabsContent>
      </Tabs>
    </>
  );
}
