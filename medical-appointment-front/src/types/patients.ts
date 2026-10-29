export interface PatientProfile {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  dateOfBirth: string;
  phoneNumber: string;
  bloodType: string;
  allergies?: string | null;
}

export interface CreatePatientInput {
  fullName: string;
  email: string;
  dateOfBirth: string; // "1990-05-15"
  phoneNumber: string;
  bloodType: string;
  allergies?: string;
}

export interface MedicalDocument {
  id: string;
  fileName: string;
  fileType: string;
  documentType: string;
  fileSize: number;
  uploadedAt: string;
  downloadUrl?: string | null;
}