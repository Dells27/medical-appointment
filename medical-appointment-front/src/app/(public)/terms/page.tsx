import type { Metadata } from "next";
import LegalPage from "@/components/shared/LegalPage";

export const metadata: Metadata = { title: "Terms & conditions · Medical Appointment" };

// Template text — have it reviewed by a legal professional before going live
export default function TermsPage() {
  return (
    <LegalPage
      title="Terms & conditions"
      updated="November 19, 2026"
      sections={[
        {
          heading: "About the service",
          paragraphs: [
            "Medical Appointment is a platform that connects patients with doctors to schedule appointments and manage medical information. It does not provide medical care itself.",
          ],
        },
        {
          heading: "Not for emergencies",
          paragraphs: [
            "Do not use this platform for medical emergencies. If you need urgent care, call your local emergency number or go to the nearest emergency room.",
          ],
        },
        {
          heading: "Accounts",
          paragraphs: [
            "You are responsible for keeping your password safe and for the activity on your account. The information you provide must be accurate and up to date.",
            "Doctors must hold a valid license to practice and provide a real license number.",
          ],
        },
        {
          heading: "Appointments",
          paragraphs: [
            "Appointments are subject to the doctor's confirmation and availability. Doctors may reschedule or cancel an appointment; you will be notified by email.",
            "Please cancel appointments you cannot attend so the time can be offered to other patients.",
          ],
        },
        {
          heading: "Medical documents",
          paragraphs: [
            "You may upload only documents that belong to you. Files must be PDF, JPG or PNG and no larger than 10 MB.",
          ],
        },
        {
          heading: "Changes to these terms",
          paragraphs: [
            "We may update these terms. Material changes will be announced on the platform before they take effect.",
          ],
        },
      ]}
    />
  );
}
