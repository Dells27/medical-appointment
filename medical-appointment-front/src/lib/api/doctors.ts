import { apiDelete, apiGet, apiPost, apiPut } from "./client";
import type {
  CreateDoctorInput,
  CreateTimeOffInput,
  Doctor,
  PublicDoctor,
  Schedule,
  ScheduleInput,
  Specialty,
  TimeOff,
  UpdateDoctorInput,
} from "@/types/doctor";

function query(params: Record<string, string | undefined>) {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value) search.set(key, value);
  }
  const text = search.toString();
  return text ? `?${text}` : "";
}

// ---------- Public (no login needed) ----------

export function getPublicDoctors(filters: { specialty?: string; search?: string } = {}) {
  return apiGet<PublicDoctor[]>(`/api/doctors/public${query(filters)}`);
}

export function getSpecialties() {
  return apiGet<Specialty[]>("/api/doctors/public/specialties");
}

// ---------- Authenticated ----------

export function getDoctors(filters: { specialty?: string; search?: string } = {}) {
  return apiGet<Doctor[]>(`/api/doctors${query(filters)}`);
}

export function getDoctorByUserId(userId: string) {
  return apiGet<Doctor>(`/api/doctors/by-user/${userId}`);
}

export function createDoctorProfile(input: CreateDoctorInput) {
  return apiPost<Doctor>("/api/doctors", input);
}

// ---------- The authenticated doctor ----------

export function getMyDoctorProfile() {
  return apiGet<Doctor>("/api/doctors/me");
}

export function updateMyDoctorProfile(input: UpdateDoctorInput) {
  return apiPut<Doctor>("/api/doctors/me", input);
}

export function replaceMySchedules(schedules: ScheduleInput[]) {
  return apiPut<Schedule[]>("/api/doctors/me/schedules", { schedules });
}

export function getMyTimeOff() {
  return apiGet<TimeOff[]>("/api/doctors/me/time-off");
}

export function addTimeOff(input: CreateTimeOffInput) {
  return apiPost<TimeOff>("/api/doctors/me/time-off", input);
}

export function deleteTimeOff(id: string) {
  return apiDelete(`/api/doctors/me/time-off/${id}`);
}
