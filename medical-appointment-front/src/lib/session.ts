import type { StoredSession } from "@/types/auth";

const KEY = "medic_session";

export function saveSession(session: StoredSession) {
  localStorage.setItem(KEY, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(KEY);
}

export function loadSession(): StoredSession | null {
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return null;

    const session = JSON.parse(raw) as StoredSession;

    // Si el token ya expiró, descartamos la sesión
    if (new Date(session.expiresAt) <= new Date()) {
      clearSession();
      return null;
    }

    return session;
  } catch {
    return null;
  }
}