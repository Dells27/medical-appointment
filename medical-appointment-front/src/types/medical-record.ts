export interface MedicalRecord {
  id: string;
  appointmentId: string;
  patientId: string;
  doctorId: string;
  appointmentDate: string;
  diagnosis?: string | null;
  treatment?: string | null;
  observations?: string | null;
  isCompleted: boolean;
  createdAt: string;
}

export interface FillNotesInput {
  diagnosis: string;
  treatment: string;
  observations?: string;
}

export interface MedicalBackground {
  patientId: string;
  chronicConditions?: string | null;
  surgeries?: string | null;
  currentMedications?: string | null;
  familyHistory?: string | null;
  allergies?: string | null;
  habits?: string | null;
  notes?: string | null;
  updatedByDoctorId?: string | null;
  updatedAt?: string | null; // null when nothing has been recorded yet
}

export type MedicalBackgroundInput = Omit<
  MedicalBackground,
  "patientId" | "updatedByDoctorId" | "updatedAt"
>;
