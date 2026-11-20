import { Badge } from "@/components/ui/badge";
import { statusLabel, statusVariant } from "@/lib/appointment-status";

export default function StatusBadge({ status }: { status: string }) {
  return <Badge variant={statusVariant(status)}>{statusLabel(status)}</Badge>;
}
