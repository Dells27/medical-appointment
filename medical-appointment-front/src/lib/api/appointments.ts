import { apiPost } from "./client";
import type { Appointment, CreateAppointmentInput } from "@/types/appointment";

export function createAppointment(token: string, input: CreateAppointmentInput) {
  return apiPost<Appointment>("/api/appointments", token, input);
}