import DocumentsPanel from "@/components/shared/DocumentsPanel";
import PageHeader from "@/components/shared/PageHeader";

export default function PatientDocumentsPage() {
  return (
    <>
      <PageHeader
        title="Medical documents"
        description="Documents you share with your doctors and the ones they issue for you."
      />
      <DocumentsPanel mode="patient" />
    </>
  );
}
