"use client";

import { useQuery } from "@tanstack/react-query";
import { useAppSelector } from "@/store/hooks";
import { getDoctors } from "@/lib/api/doctors";
import DoctorProfileForm from "@/components/shared/DoctorProfileForm";
import DoctorSchedule from "@/components/shared/DoctorSchedule";

export default function DoctorDashboard() {
  const token = useAppSelector((state) => state.auth.token);
  const user = useAppSelector((state) => state.auth.user);

  const { data: doctors, isLoading } = useQuery({
    queryKey: ["doctors"],
    queryFn: () => getDoctors(token!),
    enabled: !!token,
  });

  if (isLoading) return <p>Loading...</p>;

  // No hay endpoint "mi perfil": buscamos el nuestro en la lista por email
  const hasProfile = doctors?.some(
    (d) => d.email.toLowerCase() === user?.email.toLowerCase()
  );

  return hasProfile ? <DoctorSchedule /> : <DoctorProfileForm />;
}