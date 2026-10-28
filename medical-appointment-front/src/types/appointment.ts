export interface Appointment {
  id: string;
  patientId: string;
  doctorId: string;
  appointmentDate: string;
  appointmentTime: string;
  status: string;
  reason?: string | null;
  cancellationReason?: string | null;
  createdAt: string;
}

export interface CreateAppointmentInput {
  doctorId: string;
  appointmentDate: string; // "2026-11-05"
  appointmentTime: string; // "10:30:00"
  reason?: string;
}