import RoleGuard from "@/components/shared/RoleGuard";

export default function DoctorLayout({ children }: { children: React.ReactNode }) {
  return <RoleGuard role="Doctor">{children}</RoleGuard>;
}