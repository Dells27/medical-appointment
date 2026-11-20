// All requests go to relative /api/* paths. next.config.ts proxies them to the
// API gateway, so the browser only talks to its own origin and the httpOnly
// auth cookie is sent automatically. JavaScript never sees the token.

export class ApiError extends Error {
  constructor(message: string, public status: number) {
    super(message);
  }
}

// Called when a protected request returns 401 (expired or missing session)
let onUnauthorized: (() => void) | null = null;

export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler;
}

function defaultMessage(status: number) {
  if (status === 401) return "Your session has expired. Please sign in again.";
  if (status === 403) return "You don't have permission to do this.";
  if (status === 404) return "Not found.";
  if (status === 429) return "Too many requests. Please try again in a moment.";
  return "Something went wrong. Please try again.";
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const res = await fetch(path, { ...init, credentials: "include" });

  // Auth endpoints (login, me) answer 401 as a normal outcome — don't treat it as an expired session
  if (res.status === 401 && !path.startsWith("/api/auth/")) {
    onUnauthorized?.();
  }

  if (!res.ok) {
    const data = await res.json().catch(() => null);
    throw new ApiError(data?.message ?? defaultMessage(res.status), res.status);
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

function jsonInit(method: string, body: unknown): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body ?? {}),
  };
}

export function apiGet<T>(path: string): Promise<T> {
  return request<T>(path);
}

export function apiPost<T>(path: string, body?: unknown): Promise<T> {
  return request<T>(path, jsonInit("POST", body));
}

export function apiPut<T>(path: string, body?: unknown): Promise<T> {
  return request<T>(path, jsonInit("PUT", body));
}

export function apiDelete<T = void>(path: string): Promise<T> {
  return request<T>(path, { method: "DELETE" });
}

export function apiUpload<T>(path: string, formData: FormData): Promise<T> {
  // No Content-Type: the browser sets it with the multipart boundary
  return request<T>(path, { method: "POST", body: formData });
}
