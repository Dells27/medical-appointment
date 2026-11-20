"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ClipboardList, Pencil } from "lucide-react";
import {
  getMyBackground,
  getPatientBackground,
  updatePatientBackground,
} from "@/lib/api/medical-record";
import { formatTimestamp } from "@/lib/format";
import type { MedicalBackground, MedicalBackgroundInput } from "@/types/medical-record";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { EmptyState, ErrorState, LoadingState } from "./QueryState";

const FIELDS: { key: keyof MedicalBackgroundInput; label: string; placeholder: string }[] = [
  { key: "chronicConditions", label: "Chronic conditions", placeholder: "e.g. Hypertension (2019), type 2 diabetes" },
  { key: "surgeries", label: "Surgeries & hospitalizations", placeholder: "e.g. Appendectomy (2015)" },
  { key: "currentMedications", label: "Current medications", placeholder: "e.g. Metformin 850 mg, twice a day" },
  { key: "allergies", label: "Allergies", placeholder: "e.g. Penicillin" },
  { key: "familyHistory", label: "Family history", placeholder: "e.g. Father: heart disease" },
  { key: "habits", label: "Habits", placeholder: "e.g. Non-smoker, exercises 3x/week" },
  { key: "notes", label: "Other notes", placeholder: "" },
];

// Medical background (antecedentes). Read-only for the patient; editable by the treating doctor.
export default function MedicalBackgroundPanel(
  props: { mode: "patient"; patientAllergies?: string | null } | { mode: "doctor"; patientId: string },
) {
  const isDoctor = props.mode === "doctor";
  const patientId = isDoctor ? props.patientId : undefined;
  const queryKey = isDoctor ? ["background", patientId] : ["background", "me"];
  const [editing, setEditing] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => (isDoctor ? getPatientBackground(patientId!) : getMyBackground()),
  });

  if (isLoading) return <LoadingState />;
  if (error) return <ErrorState message={error.message} />;
  if (!data) return null;

  if (isDoctor && editing) {
    return (
      <BackgroundForm
        patientId={patientId!}
        background={data}
        queryKey={queryKey}
        onDone={() => setEditing(false)}
      />
    );
  }

  const hasData = FIELDS.some((f) => data[f.key]);

  return (
    <Card>
      <CardHeader className="flex flex-row flex-wrap items-start justify-between gap-3">
        <div className="space-y-1">
          <CardTitle>Medical history</CardTitle>
          <CardDescription>
            {data.updatedAt
              ? `Last updated ${formatTimestamp(data.updatedAt)} by the treating doctor`
              : isDoctor
                ? "Nothing recorded yet."
                : "Your doctor will complete this during your consultations."}
          </CardDescription>
        </div>
        {isDoctor && (
          <Button size="sm" variant="outline" onClick={() => setEditing(true)}>
            <Pencil />
            {hasData ? "Edit" : "Add medical history"}
          </Button>
        )}
      </CardHeader>
      <CardContent>
        {!hasData && !(props.mode === "patient" && props.patientAllergies) ? (
          <EmptyState icon={ClipboardList} title="No medical history recorded yet" />
        ) : (
          <dl className="grid gap-5 sm:grid-cols-2">
            {FIELDS.filter((f) => data[f.key]).map((f) => (
              <div key={f.key} className="space-y-1">
                <dt className="text-sm font-medium">{f.label}</dt>
                <dd className="whitespace-pre-wrap text-sm text-muted-foreground">{data[f.key]}</dd>
              </div>
            ))}
            {props.mode === "patient" && props.patientAllergies && !data.allergies && (
              <div className="space-y-1">
                <dt className="text-sm font-medium">Allergies (reported by you)</dt>
                <dd className="text-sm text-muted-foreground">{props.patientAllergies}</dd>
              </div>
            )}
          </dl>
        )}
      </CardContent>
    </Card>
  );
}

function BackgroundForm({
  patientId,
  background,
  queryKey,
  onDone,
}: {
  patientId: string;
  background: MedicalBackground;
  queryKey: unknown[];
  onDone: () => void;
}) {
  const queryClient = useQueryClient();
  const [values, setValues] = useState<MedicalBackgroundInput>(() =>
    Object.fromEntries(FIELDS.map((f) => [f.key, background[f.key] ?? ""])) as MedicalBackgroundInput,
  );

  const save = useMutation({
    mutationFn: () => updatePatientBackground(patientId, values),
    onSuccess: (saved) => {
      toast.success("Medical history saved");
      queryClient.setQueryData(queryKey, saved);
      onDone();
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Edit medical history</CardTitle>
        <CardDescription>The patient can read this information but not change it.</CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="grid gap-4 sm:grid-cols-2"
          onSubmit={(e) => {
            e.preventDefault();
            save.mutate();
          }}
        >
          {FIELDS.map((f) => (
            <div key={f.key} className="space-y-2">
              <Label htmlFor={f.key}>{f.label}</Label>
              <Textarea
                id={f.key}
                rows={3}
                placeholder={f.placeholder}
                value={values[f.key] ?? ""}
                onChange={(e) => setValues((v) => ({ ...v, [f.key]: e.target.value }))}
              />
            </div>
          ))}
          {save.error && <p className="text-sm text-red-500 sm:col-span-2">{save.error.message}</p>}
          <div className="flex gap-2 sm:col-span-2">
            <Button type="submit" disabled={save.isPending}>
              {save.isPending ? "Saving..." : "Save"}
            </Button>
            <Button type="button" variant="outline" onClick={onDone}>
              Cancel
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
