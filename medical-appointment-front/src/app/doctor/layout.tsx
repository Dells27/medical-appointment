import RoleGuard from "@/components/shared/RoleGuard";
import AppShell from "@/components/layout/AppShell";
import { DoctorProfileGate } from "@/components/shared/ProfileGate";

export default function DoctorLayout({ children }: { children: React.ReactNode }) {
  return (
    <RoleGuard role="Doctor">
      <AppShell role="Doctor">
        <DoctorProfileGate>{children}</DoctorProfileGate>
      </AppShell>
    </RoleGuard>
  );
}
