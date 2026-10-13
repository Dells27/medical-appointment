export interface AuthResponse{
    token: string;
    name: string;
    email: string;
    role: "Patient" | "Doctor";
    expiresAt: string;
}

export interface AuthUser{
    name: string;
    email: string;
    role: "Patient" | "Doctor";
}