"use client";

import { useQuery } from "@tanstack/react-query";
import { useAppSelector } from "@/store/hooks";
import { getMyHistory } from "@/lib/api/medical-record";
import { getDoctors } from "@/lib/api/doctors";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";

export default function MedicalHistoryPage() {
  const token = useAppSelector((state) => state.auth.token);

  const { data: records, isLoading, error } = useQuery({
    queryKey: ["history"],
    queryFn: () => getMyHistory(token!),
    enabled: !!token,
  });

  const { data: doctors } = useQuery({
    queryKey: ["doctors"],
    queryFn: () => getDoctors(token!),
    enabled: !!token,
  });

  function doctorName(userId: string) {
    return doctors?.find((d) => d.userId === userId)?.name ?? "Doctor";
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Medical history</h1>

      {isLoading && <p>Loading history...</p>}
      {error && <p className="text-red-500">Could not load your history.</p>}
      {records?.length === 0 && <p>You have no medical records yet.</p>}

      <div className="space-y-3">
        {records?.map((record) => (
          <Card key={record.id}>
            <CardContent className="space-y-3 pt-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">{doctorName(record.doctorId)}</p>
                  <p className="text-sm text-muted-foreground">
                    {record.appointmentDate.slice(0, 10)}
                  </p>
                </div>
                {!record.isCompleted && <Badge variant="outline">Pending notes</Badge>}
              </div>

              {record.isCompleted && (
                <div className="space-y-2 text-sm">
                  <p><span className="font-medium">Diagnosis:</span> {record.diagnosis}</p>
                  <p><span className="font-medium">Treatment:</span> {record.treatment}</p>
                  {record.observations && (
                    <p><span className="font-medium">Observations:</span> {record.observations}</p>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}