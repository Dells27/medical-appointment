import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import type { AuthUser, StoredSession } from "@/types/auth";

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  isHydrated: boolean; // true cuando ya intentamos restaurar la sesión
}

const initialState: AuthState = {
  user: null,
  token: null,
  expiresAt: null,
  isAuthenticated: false,
  isHydrated: false,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials(state, action: PayloadAction<StoredSession>) {
      state.user = action.payload.user;
      state.token = action.payload.token;
      state.expiresAt = action.payload.expiresAt;
      state.isAuthenticated = true;
      state.isHydrated = true;
    },
    restoreSession(state, action: PayloadAction<StoredSession | null>) {
      if (action.payload) {
        state.user = action.payload.user;
        state.token = action.payload.token;
        state.expiresAt = action.payload.expiresAt;
        state.isAuthenticated = true;
      }
      state.isHydrated = true;
    },
    logout(state) {
      state.user = null;
      state.token = null;
      state.expiresAt = null;
      state.isAuthenticated = false;
    },
  },
});

export const { setCredentials, restoreSession, logout } = authSlice.actions;
export default authSlice.reducer;