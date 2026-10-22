import RoleGuard from "@/components/shared/RoleGuard";
import Navbar from "@/components/shared/Navbar";

export default function DoctorLayout({ children }: { children: React.ReactNode }) {
  return (
    <RoleGuard role="Doctor">
      <Navbar />
      <main className="mx-auto max-w-5xl p-4">{children}</main>
    </RoleGuard>
  );
}