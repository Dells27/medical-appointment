import { apiGet, apiPost, apiPut } from "./client";
import type {
  Appointment,
  AvailableSlots,
  CreateAppointmentInput,
  DoctorPatient,
  DoctorStats,
  RescheduleAppointmentInput,
} from "@/types/appointment";

// ---------- Booking ----------

export function getAvailableSlots(doctorId: string, date: string) {
  return apiGet<AvailableSlots>(
    `/api/appointments/available-slots?doctorId=${doctorId}&date=${date}`,
  );
}

export function createAppointment(input: CreateAppointmentInput) {
  return apiPost<Appointment>("/api/appointments", input);
}

// ---------- Reading ----------

export function getMyAppointments() {
  return apiGet<Appointment[]>("/api/appointments/my-appointments");
}

export function getMySchedule() {
  return apiGet<Appointment[]>("/api/appointments/my-schedule");
}

// ---------- State changes ----------

export function cancelAppointment(id: string, reason: string) {
  return apiPut<Appointment>(`/api/appointments/${id}/cancel`, { reason });
}

export function confirmAppointment(id: string) {
  return apiPut<Appointment>(`/api/appointments/${id}/confirm`);
}

export function rescheduleAppointment(id: string, input: RescheduleAppointmentInput) {
  return apiPut<Appointment>(`/api/appointments/${id}/reschedule`, input);
}

export function completeAppointment(id: string) {
  return apiPut<Appointment>(`/api/appointments/${id}/complete`);
}

// ---------- Doctor: patients and statistics ----------

export function getMyPatients() {
  return apiGet<DoctorPatient[]>("/api/appointments/my-patients");
}

export function getAppointmentsWithPatient(patientId: string) {
  return apiGet<Appointment[]>(`/api/appointments/my-patients/${patientId}/appointments`);
}

export function getMyStats() {
  return apiGet<DoctorStats>("/api/appointments/my-stats");
}
