import { apiGet, apiPost, apiPut, apiUpload } from "./client";
import type {
  CreatePatientInput,
  MedicalDocument,
  PatientProfile,
  UpdatePatientInput,
  UploadDocumentInput,
} from "@/types/patients";

function uploadUrl(base: string, input: UploadDocumentInput) {
  const params = new URLSearchParams({ documentType: input.documentType });
  if (input.description) params.set("description", input.description);
  if (input.appointmentId) params.set("appointmentId", input.appointmentId);
  return `${base}?${params.toString()}`;
}

function toFormData(file: File) {
  const formData = new FormData();
  formData.append("file", file);
  return formData;
}

// ---------- The authenticated patient ----------

export function getPatientProfile() {
  return apiGet<PatientProfile>("/api/patients/profile");
}

export function createPatientProfile(input: CreatePatientInput) {
  return apiPost<PatientProfile>("/api/patients", input);
}

export function updatePatientProfile(input: UpdatePatientInput) {
  return apiPut<PatientProfile>("/api/patients/profile", input);
}

export function getMyDocuments() {
  return apiGet<MedicalDocument[]>("/api/patients/documents");
}

export function uploadMyDocument(input: UploadDocumentInput) {
  return apiUpload<MedicalDocument>(uploadUrl("/api/patients/documents", input), toFormData(input.file));
}

// ---------- A doctor working with one of their patients ----------

export function getPatientForDoctor(patientId: string) {
  return apiGet<PatientProfile>(`/api/patients/${patientId}`);
}

export function getPatientDocuments(patientId: string) {
  return apiGet<MedicalDocument[]>(`/api/patients/${patientId}/documents`);
}

export function uploadPatientDocument(patientId: string, input: UploadDocumentInput) {
  return apiUpload<MedicalDocument>(
    uploadUrl(`/api/patients/${patientId}/documents`, input),
    toFormData(input.file),
  );
}
