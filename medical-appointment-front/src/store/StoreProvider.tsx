"use client";

import { useEffect, useState } from "react";
import { Provider } from "react-redux";
import { makeStore } from "./store";
import { logout, sessionChecked } from "./slices/authslices";
import { getMe } from "@/lib/api/auth";
import { setUnauthorizedHandler } from "@/lib/api/client";

export default function StoreProvider({ children }: { children: React.ReactNode }) {
  // Lazy initializer: one store per browser session, created on first render
  const [store] = useState(makeStore);

  useEffect(() => {
    // Any protected request that returns 401 ends the session in the UI
    setUnauthorizedHandler(() => store.dispatch(logout()));

    // Restore the session from the httpOnly cookie
    getMe()
      .then(({ userId, name, email, role }) =>
        store.dispatch(sessionChecked({ userId, name, email, role })),
      )
      .catch(() => store.dispatch(sessionChecked(null)));

    return () => setUnauthorizedHandler(null);
  }, [store]);

  return <Provider store={store}>{children}</Provider>;
}
