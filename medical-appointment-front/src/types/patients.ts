export interface PatientProfile {
  id: string;
  userId: string;
  name: string;
  email: string;
  dateOfBirth: string;
  phoneNumber: string;
  bloodType: string;
  allergies?: string | null;
}

export interface CreatePatientInput {
  name: string;
  email: string;
  dateOfBirth: string; // "1990-05-15"
  phoneNumber: string;
  bloodType: string;
  allergies?: string;
}

export interface UpdatePatientInput {
  name: string;
  phoneNumber: string;
  dateOfBirth: string;
  bloodType: string;
  allergies?: string;
}

export interface MedicalDocument {
  id: string;
  fileName: string;
  fileType: string;
  documentType: string;
  fileSize: number;
  uploadedByRole: "Patient" | "Doctor";
  uploadedByName?: string | null;
  appointmentId?: string | null;
  description?: string | null;
  uploadedAt: string;
  downloadUrl?: string | null;
}

export interface UploadDocumentInput {
  file: File;
  documentType: string;
  description?: string;
  appointmentId?: string;
}
