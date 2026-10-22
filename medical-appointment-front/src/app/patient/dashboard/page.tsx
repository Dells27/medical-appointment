"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useAppSelector } from "@/store/hooks";
import { getDoctors } from "@/lib/api/doctors";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function PatientDashboard() {
  const token = useAppSelector((state) => state.auth.token);
  const [search, setSearch] = useState("");

  const { data: doctors, isLoading, error } = useQuery({
    queryKey: ["doctors"],
    queryFn: () => getDoctors(token!),
    enabled: !!token,
  });

  const filtered = doctors?.filter((d) =>
    `${d.name} ${d.specialty}`.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <h1 className="text-2xl font-semibold">Find a doctor</h1>
        <Input
          placeholder="Search by name or specialty"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-md"
        />
      </div>

      {isLoading && <p>Loading doctors...</p>}
      {error && <p className="text-red-500">Could not load doctors.</p>}
      {filtered?.length === 0 && <p>No doctors found.</p>}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {filtered?.map((doctor) => (
          <Card key={doctor.id}>
            <CardHeader>
              <CardTitle>{doctor.name}</CardTitle>
              <p className="text-sm text-muted-foreground">{doctor.specialty}</p>
            </CardHeader>
            <CardContent className="space-y-3">
              {doctor.bio && <p className="text-sm">{doctor.bio}</p>}
              <Button className="w-full" disabled>
                Book appointment
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}