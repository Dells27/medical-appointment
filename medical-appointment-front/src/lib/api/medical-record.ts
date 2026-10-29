import { apiGet, apiPut } from "./client";
import type { FillNotesInput, MedicalRecord } from "@/types/medical-record";

export function getRecordByAppointment(token: string, appointmentId: string) {
  return apiGet<MedicalRecord>(`/api/medical-records/by-appointment/${appointmentId}`, token);
}

export function fillNotes(token: string, recordId: string, input: FillNotesInput) {
  return apiPut<MedicalRecord>(`/api/medical-records/${recordId}/notes`, token, input);
}

export function getMyHistory(token: string) {
  return apiGet<MedicalRecord[]>("/api/medical-records/my-history", token);
}