"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { logout } from "@/store/slices/authslices";
import { Button } from "@/components/ui/button";

export default function Navbar() {
  const user = useAppSelector((state) => state.auth.user);
  const dispatch = useAppDispatch();
  const router = useRouter();

  const home = user?.role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard";

  function handleLogout() {
    dispatch(logout());
    router.push("/login");
  }

  return (
    <header className="border-b">
      <nav className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
        <Link href={home} className="font-semibold">
          Medical Appointment
        </Link>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">{user?.name}</span>
          <Button variant="outline" size="sm" onClick={handleLogout}>
            Sign out
          </Button>
        </div>
      </nav>
    </header>
  );
}