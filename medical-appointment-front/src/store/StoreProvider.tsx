"use client";

import { useEffect, useRef } from "react";
import { Provider } from "react-redux";
import { makeStore, AppStore } from "./store";
import { restoreSession } from "./slices/authslices";
import { clearSession, loadSession, saveSession } from "@/lib/session";

export default function StoreProvider({ children }: { children: React.ReactNode }) {
  const storeRef = useRef<AppStore | null>(null);

  if (!storeRef.current) {
    storeRef.current = makeStore();
  }

  useEffect(() => {
    const store = storeRef.current!;

    // 1. Al arrancar: recuperar la sesión guardada
    store.dispatch(restoreSession(loadSession()));

    // 2. Después: cada cambio en auth se refleja en localStorage
    const unsubscribe = store.subscribe(() => {
      const { user, token, expiresAt } = store.getState().auth;

      if (user && token && expiresAt) {
        saveSession({ user, token, expiresAt });
      } else {
        clearSession();
      }
    });

    return unsubscribe;
  }, []);

  return <Provider store={storeRef.current}>{children}</Provider>;
}