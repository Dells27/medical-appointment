import type { Metadata } from "next";
import { Clock, Mail, MapPin, Phone } from "lucide-react";
import PageHeader from "@/components/shared/PageHeader";

export const metadata: Metadata = { title: "Contact · Medical Appointment" };

// TODO: replace these placeholders with the real support details
const CONTACT = [
  {
    icon: Mail,
    title: "Email",
    lines: ["support@medicalappointment.com"],
    href: "mailto:support@medicalappointment.com",
  },
  { icon: Phone, title: "Phone", lines: ["+1 (555) 010-2030"], href: "tel:+15550102030" },
  { icon: Clock, title: "Support hours", lines: ["Monday – Friday, 8:00 AM – 6:00 PM", "Saturday, 9:00 AM – 1:00 PM"] },
  { icon: MapPin, title: "Address", lines: ["123 Health Avenue, Suite 400", "Your City"] },
];

export default function ContactPage() {
  return (
    <div className="mx-auto max-w-4xl space-y-8 px-4 py-12">
      <PageHeader
        title="Contact us"
        description="Questions about an appointment or your account? Our support team is here to help."
      />

      <div className="grid gap-4 sm:grid-cols-2">
        {CONTACT.map((item) => (
          <div key={item.title} className="flex gap-4 rounded-lg border bg-card p-5">
            <item.icon className="mt-0.5 size-5 shrink-0 text-primary" />
            <div className="space-y-1">
              <p className="font-medium">{item.title}</p>
              {item.lines.map((line) =>
                item.href ? (
                  <a key={line} href={item.href} className="block text-sm text-muted-foreground hover:text-foreground">
                    {line}
                  </a>
                ) : (
                  <p key={line} className="text-sm text-muted-foreground">
                    {line}
                  </p>
                ),
              )}
            </div>
          </div>
        ))}
      </div>

      <div className="rounded-lg border border-amber-300/60 bg-amber-50 p-4 text-sm text-amber-900 dark:border-amber-500/30 dark:bg-amber-500/10 dark:text-amber-200">
        In case of a medical emergency, do not use this platform. Call your local emergency number
        immediately.
      </div>
    </div>
  );
}
