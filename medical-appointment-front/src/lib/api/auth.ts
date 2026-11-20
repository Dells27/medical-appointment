import { apiGet, apiPost } from "./client";
import type { AuthUserResponse } from "@/types/auth";

export function login(email: string, password: string) {
  return apiPost<AuthUserResponse>("/api/auth/login", { email, password });
}

export function registerPatient(name: string, email: string, password: string) {
  return apiPost<AuthUserResponse>("/api/auth/register/patient", { name, email, password });
}

export function registerDoctor(name: string, email: string, password: string) {
  return apiPost<AuthUserResponse>("/api/auth/register/doctor", { name, email, password });
}

export function getMe() {
  return apiGet<AuthUserResponse>("/api/auth/me");
}

export function logoutRequest() {
  return apiPost<void>("/api/auth/logout");
}
