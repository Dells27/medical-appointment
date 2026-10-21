"use client";

import { useRouter } from "next/navigation";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { logout } from "@/store/slices/authslices";
import { Button } from "@/components/ui/button";

export default function PatientDashboard() {
  const user = useAppSelector((state) => state.auth.user);
  const dispatch = useAppDispatch();
  const router = useRouter();

  function handleLogout() {
    dispatch(logout());
    router.push("/login");
  }

  return (
    <div className="space-y-4 p-8">
      <h1 className="text-2xl">Patient dashboard: {user?.name}</h1>
      <Button onClick={handleLogout}>Sign out</Button>
    </div>
  );
}