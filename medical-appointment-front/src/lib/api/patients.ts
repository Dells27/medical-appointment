import { apiGet, apiPost, apiUpload } from "./client";
import type { CreatePatientInput, MedicalDocument, PatientProfile } from "@/types/patients";

export function getPatientProfile(token: string) {
  return apiGet<PatientProfile>("/api/patients/profile", token);
}

export function createPatientProfile(token: string, input: CreatePatientInput) {
  return apiPost<PatientProfile>("/api/patients", token, input);
}

export function getDocuments(token: string) {
  return apiGet<MedicalDocument[]>("/api/patients/documents", token);
}

export function uploadDocument(token: string, file: File, documentType: string) {
  const formData = new FormData();
  formData.append("file", file);
  return apiUpload<MedicalDocument>(
    `/api/patients/documents?documentType=${encodeURIComponent(documentType)}`,
    token,
    formData
  );
}