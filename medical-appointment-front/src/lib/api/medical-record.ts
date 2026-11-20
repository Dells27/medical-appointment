import { apiGet, apiPut } from "./client";
import type {
  FillNotesInput,
  MedicalBackground,
  MedicalBackgroundInput,
  MedicalRecord,
} from "@/types/medical-record";

// ---------- Consultation records ----------

export function getRecordByAppointment(appointmentId: string) {
  return apiGet<MedicalRecord>(`/api/medical-records/by-appointment/${appointmentId}`);
}

export function fillNotes(recordId: string, input: FillNotesInput) {
  return apiPut<MedicalRecord>(`/api/medical-records/${recordId}/notes`, input);
}

export function getMyHistory() {
  return apiGet<MedicalRecord[]>("/api/medical-records/my-history");
}

export function getMyConsultations() {
  return apiGet<MedicalRecord[]>("/api/medical-records/my-consultations");
}

export function getPatientHistory(patientId: string) {
  return apiGet<MedicalRecord[]>(`/api/medical-records/patient/${patientId}`);
}

// ---------- Medical background ----------

export function getMyBackground() {
  return apiGet<MedicalBackground>("/api/medical-records/background/me");
}

export function getPatientBackground(patientId: string) {
  return apiGet<MedicalBackground>(`/api/medical-records/background/${patientId}`);
}

export function updatePatientBackground(patientId: string, input: MedicalBackgroundInput) {
  return apiPut<MedicalBackground>(`/api/medical-records/background/${patientId}`, input);
}
