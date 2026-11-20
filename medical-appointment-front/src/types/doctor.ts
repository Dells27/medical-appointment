export interface Schedule {
  id: string;
  dayOfWeek: number; // 0 = Sunday … 6 = Saturday
  startTime: string; // "08:00:00"
  endTime: string;
}

export interface Doctor {
  id: string;
  userId: string;
  name: string;
  email: string;
  specialty: string;
  licenseNumber: string;
  photoUrl?: string | null;
  bio?: string | null;
  schedules: Schedule[];
}

// Anonymous view: no email or license number
export interface PublicDoctor {
  userId: string;
  name: string;
  specialty: string;
  photoUrl?: string | null;
  bio?: string | null;
  schedules: Schedule[];
}

export interface Specialty {
  name: string;
  doctorCount: number;
}

export interface CreateDoctorInput {
  name: string;
  email: string;
  specialty: string;
  licenseNumber: string;
}

export interface UpdateDoctorInput {
  name: string;
  specialty: string;
  bio?: string;
  photoUrl?: string;
}

export interface ScheduleInput {
  dayOfWeek: number;
  startTime: string; // "08:00:00"
  endTime: string;
}

export type TimeOffType = "Vacation" | "DayOff" | "BlockedHours";

export interface TimeOff {
  id: string;
  type: TimeOffType;
  startDate: string; // "2026-12-24"
  endDate: string;
  startTime?: string | null;
  endTime?: string | null;
  reason?: string | null;
}

export interface CreateTimeOffInput {
  type: TimeOffType;
  startDate: string;
  endDate: string;
  startTime?: string;
  endTime?: string;
  reason?: string;
}
