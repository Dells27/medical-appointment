// Must match MedicalDocument.PatientDocumentTypes / DoctorDocumentTypes in the Patient Service

export const PATIENT_DOCUMENT_TYPES = [
  { value: "LabResult", label: "Lab results" },
  { value: "Imaging", label: "X-ray / ultrasound" },
  { value: "PreviousExam", label: "Previous exam" },
  { value: "ExternalPrescription", label: "Prescription from another doctor" },
  { value: "ExternalReport", label: "External medical report" },
  { value: "Other", label: "Other" },
] as const;

export const DOCTOR_DOCUMENT_TYPES = [
  { value: "Prescription", label: "Prescription" },
  { value: "LabOrder", label: "Lab order" },
  { value: "ImagingOrder", label: "X-ray / imaging order" },
  { value: "DiagnosisReport", label: "Diagnosis / medical report" },
  { value: "MedicalInstructions", label: "Instructions & recommendations" },
  { value: "MedicalCertificate", label: "Medical certificate" },
] as const;

// Older uploads used these values
const LEGACY_LABELS: Record<string, string> = {
  Exam: "Exam",
  "X-ray": "X-ray",
};

const LABELS: Record<string, string> = Object.fromEntries(
  [...PATIENT_DOCUMENT_TYPES, ...DOCTOR_DOCUMENT_TYPES].map((t) => [t.value, t.label]),
);

export function documentTypeLabel(value: string) {
  return LABELS[value] ?? LEGACY_LABELS[value] ?? value;
}

export const ACCEPTED_FILE_TYPES = "application/pdf,image/jpeg,image/png";
export const MAX_FILE_SIZE = 10 * 1024 * 1024;
