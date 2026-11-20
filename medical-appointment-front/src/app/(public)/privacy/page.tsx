import type { Metadata } from "next";
import LegalPage from "@/components/shared/LegalPage";

export const metadata: Metadata = { title: "Privacy policy · Medical Appointment" };

// Template text — have it reviewed by a legal professional before going live
export default function PrivacyPage() {
  return (
    <LegalPage
      title="Privacy policy"
      updated="November 19, 2026"
      sections={[
        {
          heading: "Information we collect",
          paragraphs: [
            "Account information: your name, email address and password (stored as a secure hash).",
            "Profile information: for patients, date of birth, phone number, blood type and allergies; for doctors, specialty, license number and professional details.",
            "Health information: appointments, clinical notes, medical background and the documents you or your doctors upload.",
          ],
        },
        {
          heading: "How we use your information",
          paragraphs: [
            "To schedule and manage appointments, to let the doctors who treat you review your history, and to send you notifications about your appointments.",
            "We do not sell your personal information or use your health data for advertising.",
          ],
        },
        {
          heading: "Who can see your health information",
          paragraphs: [
            "Your clinical information is visible to you and to the doctors you have booked appointments with. Doctors without an appointment with you cannot access it.",
            "Documents are stored in encrypted cloud storage and shared only through temporary download links.",
          ],
        },
        {
          heading: "Security",
          paragraphs: [
            "Your session is kept in a secure, http-only cookie that scripts cannot read. All traffic is encrypted in transit.",
          ],
        },
        {
          heading: "Your rights",
          paragraphs: [
            "You can review and update your personal information from your profile at any time. To request a copy or the deletion of your data, contact our support team.",
          ],
        },
        {
          heading: "Contact",
          paragraphs: ["For privacy questions, write to support@medicalappointment.com."],
        },
      ]}
    />
  );
}
