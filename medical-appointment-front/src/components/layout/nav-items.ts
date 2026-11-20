import {
  CalendarCheck,
  CalendarDays,
  Clock,
  FileText,
  History,
  LayoutDashboard,
  type LucideIcon,
  NotebookPen,
  Search,
  Stethoscope,
  UserRound,
  Users,
} from "lucide-react";
import type { Role } from "@/types/auth";

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
}

export const NAV_ITEMS: Record<Role, NavItem[]> = {
  Patient: [
    { href: "/patient/dashboard", label: "Dashboard", icon: LayoutDashboard },
    { href: "/patient/doctors", label: "Find doctors", icon: Search },
    { href: "/patient/appointments", label: "My appointments", icon: CalendarCheck },
    { href: "/patient/history", label: "Medical history", icon: History },
    { href: "/patient/documents", label: "Medical documents", icon: FileText },
    { href: "/patient/profile", label: "Profile", icon: UserRound },
  ],
  Doctor: [
    { href: "/doctor/dashboard", label: "Dashboard", icon: LayoutDashboard },
    { href: "/doctor/agenda", label: "My agenda", icon: CalendarDays },
    { href: "/doctor/appointments", label: "My appointments", icon: CalendarCheck },
    { href: "/doctor/patients", label: "Patients", icon: Users },
    { href: "/doctor/availability", label: "Schedule & availability", icon: Clock },
    { href: "/doctor/consultations", label: "Consultation history", icon: NotebookPen },
    { href: "/doctor/profile", label: "Professional profile", icon: Stethoscope },
  ],
};
