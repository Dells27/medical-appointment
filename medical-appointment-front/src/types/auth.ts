export type Role = "Patient" | "Doctor";

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  role: Role;
}

// Returned by login, register and /api/auth/me. The JWT itself stays in an httpOnly cookie.
export interface AuthUserResponse extends AuthUser {
  expiresAt: string | null;
}
