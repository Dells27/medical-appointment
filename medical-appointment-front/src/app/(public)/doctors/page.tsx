"use client";

import { Suspense } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useAppSelector } from "@/store/hooks";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import DoctorSearch from "@/components/shared/DoctorSearch";
import PageHeader from "@/components/shared/PageHeader";

function PublicDoctors() {
  const searchParams = useSearchParams();
  const { user, isAuthenticated } = useAppSelector((state) => state.auth);

  // Patients book from their own area; everyone else is invited to sign in
  const bookHref =
    isAuthenticated && user?.role === "Patient" ? "/patient/doctors" : "/login";

  return (
    <DoctorSearch
      initialSpecialty={searchParams.get("specialty") ?? ""}
      renderAction={() => (
        <Link href={bookHref} className={cn(buttonVariants(), "w-full")}>
          {isAuthenticated ? "Book appointment" : "Sign in to book"}
        </Link>
      )}
    />
  );
}

export default function PublicDoctorsPage() {
  return (
    <div className="mx-auto max-w-6xl space-y-8 px-4 py-12">
      <PageHeader
        title="Our doctors"
        description="Browse our specialists and their weekly availability."
      />
      <Suspense>
        <PublicDoctors />
      </Suspense>
    </div>
  );
}
