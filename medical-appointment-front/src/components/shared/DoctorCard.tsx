import { Clock } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { DAY_NAMES, formatTime, initials } from "@/lib/format";
import type { PublicDoctor } from "@/types/doctor";

// Summarizes weekly hours as "Mon, Tue, Thu"
function workingDays(doctor: PublicDoctor) {
  const days = [...new Set(doctor.schedules.map((s) => s.dayOfWeek))].sort();
  return days.map((d) => DAY_NAMES[d].slice(0, 3)).join(", ");
}

function hoursRange(doctor: PublicDoctor) {
  if (doctor.schedules.length === 0) return null;
  const start = doctor.schedules.map((s) => s.startTime).sort()[0];
  const end = doctor.schedules.map((s) => s.endTime).sort().at(-1)!;
  return `${formatTime(start)} – ${formatTime(end)}`;
}

export default function DoctorCard({
  doctor,
  action,
}: {
  doctor: PublicDoctor;
  action?: React.ReactNode;
}) {
  const days = workingDays(doctor);
  const hours = hoursRange(doctor);

  return (
    <Card className="h-full">
      <CardContent className="flex h-full flex-col gap-4 pt-6">
        <div className="flex items-start gap-3">
          <span className="flex size-12 shrink-0 items-center justify-center rounded-full bg-primary/10 font-medium text-primary">
            {initials(doctor.name)}
          </span>
          <div className="min-w-0">
            <p className="truncate font-medium">{doctor.name}</p>
            <Badge variant="secondary" className="mt-1">
              {doctor.specialty}
            </Badge>
          </div>
        </div>

        {doctor.bio && <p className="line-clamp-3 text-sm text-muted-foreground">{doctor.bio}</p>}

        <div className="mt-auto space-y-3">
          <p className="flex items-center gap-2 text-xs text-muted-foreground">
            <Clock className="size-3.5" />
            {days ? `${days} · ${hours}` : "Availability not published yet"}
          </p>
          {action}
        </div>
      </CardContent>
    </Card>
  );
}
