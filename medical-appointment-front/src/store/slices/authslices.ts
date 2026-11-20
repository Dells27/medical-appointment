import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import type { AuthUser } from "@/types/auth";

// Only who the user is lives here. The token is in an httpOnly cookie that
// JavaScript cannot read, so it is never stored in Redux or localStorage.
interface AuthState {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isHydrated: boolean; // true once we asked the server whether a session exists
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isHydrated: false,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    // After login or register
    setUser(state, action: PayloadAction<AuthUser>) {
      state.user = action.payload;
      state.isAuthenticated = true;
      state.isHydrated = true;
    },
    // Result of GET /api/auth/me on page load
    sessionChecked(state, action: PayloadAction<AuthUser | null>) {
      state.user = action.payload;
      state.isAuthenticated = action.payload !== null;
      state.isHydrated = true;
    },
    logout(state) {
      state.user = null;
      state.isAuthenticated = false;
    },
  },
});

export const { setUser, sessionChecked, logout } = authSlice.actions;
export default authSlice.reducer;
