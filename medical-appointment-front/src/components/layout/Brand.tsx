import Link from "next/link";
import { Stethoscope } from "lucide-react";

export default function Brand({ href = "/" }: { href?: string }) {
  return (
    <Link href={href} className="flex items-center gap-2 font-semibold">
      <span className="flex size-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
        <Stethoscope className="size-4" />
      </span>
      <span>Medical Appointment</span>
    </Link>
  );
}
