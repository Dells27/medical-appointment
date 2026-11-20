import RoleGuard from "@/components/shared/RoleGuard";
import AppShell from "@/components/layout/AppShell";
import { PatientProfileGate } from "@/components/shared/ProfileGate";

export default function PatientLayout({ children }: { children: React.ReactNode }) {
  return (
    <RoleGuard role="Patient">
      <AppShell role="Patient">
        <PatientProfileGate>{children}</PatientProfileGate>
      </AppShell>
    </RoleGuard>
  );
}
