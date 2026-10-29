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