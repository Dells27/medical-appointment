import Link from "next/link";
import {
  CalendarCheck,
  ClipboardList,
  FileText,
  ShieldCheck,
  Stethoscope,
  UserRound,
} from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const FEATURES = [
  {
    icon: CalendarCheck,
    title: "Book in minutes",
    text: "See each doctor's real availability and pick the time that works for you.",
  },
  {
    icon: ClipboardList,
    title: "Your history in one place",
    text: "Medical background, past consultations and diagnoses, always available.",
  },
  {
    icon: FileText,
    title: "Share your documents",
    text: "Upload lab results, X-rays or reports, and receive prescriptions and orders from your doctor.",
  },
  {
    icon: ShieldCheck,
    title: "Private by design",
    text: "Only you and the doctors who treat you can see your clinical information.",
  },
];

export default function HomePage() {
  return (
    <>
      <section className="border-b bg-gradient-to-b from-primary/5 to-background">
        <div className="mx-auto grid max-w-6xl items-center gap-10 px-4 py-16 md:grid-cols-2 md:py-24">
          <div className="space-y-6">
            <h1 className="text-4xl font-semibold tracking-tight md:text-5xl">
              Care that fits your schedule
            </h1>
            <p className="text-lg text-muted-foreground">
              Find a specialist, book an appointment and keep your medical records organized — all
              from one place.
            </p>
            <div className="flex flex-wrap gap-3">
              <Link href="/register/patient" className={buttonVariants({ size: "lg" })}>
                Book an appointment
              </Link>
              <Link href="/doctors" className={buttonVariants({ size: "lg", variant: "outline" })}>
                Browse doctors
              </Link>
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-xl border bg-card p-6 shadow-sm">
              <UserRound className="mb-3 size-6 text-primary" />
              <p className="font-medium">For patients</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Book, follow and manage your appointments and documents.
              </p>
              <Link
                href="/register/patient"
                className={cn(buttonVariants({ variant: "link" }), "mt-2 px-0")}
              >
                Create a patient account
              </Link>
            </div>
            <div className="rounded-xl border bg-card p-6 shadow-sm">
              <Stethoscope className="mb-3 size-6 text-primary" />
              <p className="font-medium">For doctors</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Manage your agenda, availability, patients and clinical notes.
              </p>
              <Link
                href="/register/doctor"
                className={cn(buttonVariants({ variant: "link" }), "mt-2 px-0")}
              >
                Join as a doctor
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 py-16">
        <h2 className="mb-8 text-2xl font-semibold tracking-tight">Why patients use it</h2>
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {FEATURES.map((feature) => (
            <div key={feature.title} className="space-y-2">
              <feature.icon className="size-6 text-primary" />
              <p className="font-medium">{feature.title}</p>
              <p className="text-sm text-muted-foreground">{feature.text}</p>
            </div>
          ))}
        </div>
      </section>
    </>
  );
}
