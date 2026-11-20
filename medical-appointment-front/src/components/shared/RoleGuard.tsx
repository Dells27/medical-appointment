"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { Loader2 } from "lucide-react";
import { useAppSelector } from "@/store/hooks";

interface RoleGuardProps {
  role: "Patient" | "Doctor";
  children: React.ReactNode;
}

// Client-side guard for the patient and doctor areas. The real protection is on the
// server (gateway + services validate the JWT); this only keeps the UI consistent.
export default function RoleGuard({ role, children }: RoleGuardProps) {
  const router = useRouter();
  const user = useAppSelector((state) => state.auth.user);
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);
  const isHydrated = useAppSelector((state) => state.auth.isHydrated);

  const isAllowed = isAuthenticated && user?.role === role;

  useEffect(() => {
    // Still asking the server whether a session exists
    if (!isHydrated) return;

    if (!isAuthenticated) {
      router.replace("/login");
    } else if (user?.role !== role) {
      // Signed in with the other role: send them to their own dashboard
      router.replace(user?.role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard");
    }
  }, [isHydrated, isAuthenticated, user, role, router]);

  if (!isHydrated || !isAllowed) {
    return (
      <div className="flex min-h-screen items-center justify-center text-muted-foreground">
        <Loader2 className="size-5 animate-spin" />
      </div>
    );
  }

  return <>{children}</>;
}
