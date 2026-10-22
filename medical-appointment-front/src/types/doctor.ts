export interface Schedule {
  id: string;
  dayOfWeek: number;
  startTime: string;
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