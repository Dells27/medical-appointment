"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAppSelector } from "@/store/hooks";

interface RoleGuardProps {
  role: "Patient" | "Doctor";
  children: React.ReactNode;
}

export default function RoleGuard({ role, children }: RoleGuardProps) {
  const router = useRouter();
  const user = useAppSelector((state) => state.auth.user);
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);
  const isHydrated = useAppSelector((state) => state.auth.isHydrated);

  const isAllowed = isAuthenticated && user?.role === role;

  useEffect(() => {
    // Todavía no revisamos localStorage: no decidir nada aún
    if (!isHydrated) return;

    if (!isAuthenticated) {
      router.replace("/login");
    } else if (user?.role !== role) {
      // Logueado pero con otro rol: mandarlo a su propio dashboard
      router.replace(user?.role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard");
    }
  }, [isHydrated, isAuthenticated, user, role, router]);

  if (!isHydrated || !isAllowed) {
    return <p className="p-8">Loading...</p>;
  }

  return <>{children}</>;
}