import { apiGet } from "./client";
import type { Doctor } from "@/types/doctor";

export function getDoctors(token: string) {
  return apiGet<Doctor[]>("/api/doctors", token);
}