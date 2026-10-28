import { apiGet, apiPost, apiPut } from "./client";
import type { Appointment, CreateAppointmentInput } from "@/types/appointment";

export function createAppointment(token: string, input: CreateAppointmentInput) {
  return apiPost<Appointment>("/api/appointments", token, input);
}

export function getMyAppointments(token: string) {
  return apiGet<Appointment[]>("/api/appointments/my-appointments", token);
}

export function cancelAppointment(token: string, id: string, reason: string) {
  return apiPut<Appointment>(`/api/appointments/${id}/cancel`, token, { reason });
}