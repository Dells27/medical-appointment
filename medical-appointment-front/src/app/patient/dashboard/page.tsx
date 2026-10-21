"use client";

import { useAppSelector } from "@/store/hooks";

export default function PatientDashboard() {
  const user = useAppSelector((state) => state.auth.user);
  return <h1 className="p-8 text-2xl">Dashboard paciente: {user?.name}</h1>;
}