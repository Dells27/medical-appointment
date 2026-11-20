"use client";

import BookingDialog from "@/components/shared/BookingDialog";
import DoctorSearch from "@/components/shared/DoctorSearch";
import PageHeader from "@/components/shared/PageHeader";

export default function FindDoctorsPage() {
  return (
    <>
      <PageHeader
        title="Find a doctor"
        description="Search by name or specialty and book a time that suits you."
      />
      <DoctorSearch renderAction={(doctor) => <BookingDialog doctor={doctor} />} />
    </>
  );
}
