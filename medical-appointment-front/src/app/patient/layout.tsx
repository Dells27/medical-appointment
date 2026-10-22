import RoleGuard from "@/components/shared/RoleGuard";

export default function PatientLayout({ children }: { children: React.ReactNode }) {
  return <RoleGuard role="Patient">{children}</RoleGuard>;
}