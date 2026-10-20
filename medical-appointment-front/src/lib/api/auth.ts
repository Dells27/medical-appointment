import type { AuthResponse } from "@/types/auth";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

async function post<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  const data = await res.json();

  if (!res.ok) {
    throw new Error(data.message ?? "Error inesperado");
  }

  return data as T;
}

export function login(email: string, password: string) {
  return post<AuthResponse>("/api/auth/login", { email, password });
}

export function registerPatient(name: string, email: string, password: string) {
  return post<AuthResponse>("/api/auth/register/patient", { name, email, password });
}

export function registerDoctor(name: string, email: string, password: string) {
  return post<AuthResponse>("/api/auth/register/doctor", { name, email, password });
}