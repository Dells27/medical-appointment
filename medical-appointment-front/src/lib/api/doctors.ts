import { apiGet, apiPost } from "./client";
import type { CreateDoctorInput, Doctor } from "@/types/doctor";

export function getDoctors(token: string) {
  return apiGet<Doctor[]>("/api/doctors", token);
}

export function createDoctorProfile(token: string, input: CreateDoctorInput) {
  return apiPost<Doctor>("/api/doctors", token, input);
}