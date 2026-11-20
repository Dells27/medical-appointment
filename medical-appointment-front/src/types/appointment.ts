export type AppointmentStatus = "Scheduled" | "Confirmed" | "Completed" | "Cancelled" | "NoShow";

export interface Appointment {
  id: string;
  patientId: string;
  doctorId: string;
  patientName?: string | null;
  patientEmail?: string | null;
  appointmentDate: string; // ISO date at midnight UTC — a calendar date
  appointmentTime: string; // "10:30:00"
  status: AppointmentStatus;
  reason?: string | null;
  cancellationReason?: string | null;
  cancelledByRole?: "Patient" | "Doctor" | null;
  rescheduledAt?: string | null;
  createdAt: string;
}

export interface CreateAppointmentInput {
  doctorId: string;
  appointmentDate: string; // "2026-11-05"
  appointmentTime: string; // "10:30:00"
  reason?: string;
}

export interface RescheduleAppointmentInput {
  appointmentDate: string;
  appointmentTime: string;
}

export interface AvailableSlots {
  doctorId: string;
  date: string;
  slotMinutes: number;
  slots: string[]; // "09:00:00"
}

export interface DoctorPatient {
  patientId: string;
  patientName?: string | null;
  patientEmail?: string | null;
  completedVisits: number;
  totalAppointments: number;
  lastVisit?: string | null;
  nextAppointmentDate?: string | null;
  nextAppointmentTime?: string | null;
}

export interface MonthlyCount {
  month: string; // "2026-11"
  completed: number;
  cancelled: number;
}

export interface DoctorStats {
  totalAppointments: number;
  byStatus: Record<AppointmentStatus, number>;
  patientsAttended: number;
  upcoming: number;
  today: number;
  pendingConfirmation: number;
  cancellationRate: number;
  lastSixMonths: MonthlyCount[];
}
