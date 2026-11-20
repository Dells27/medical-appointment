"use client";

import { useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Download, FileImage, FileText, Upload } from "lucide-react";
import {
  getMyDocuments,
  getPatientDocuments,
  uploadMyDocument,
  uploadPatientDocument,
} from "@/lib/api/patients";
import {
  ACCEPTED_FILE_TYPES,
  DOCTOR_DOCUMENT_TYPES,
  MAX_FILE_SIZE,
  PATIENT_DOCUMENT_TYPES,
  documentTypeLabel,
} from "@/lib/document-types";
import { formatFileSize, formatTimestamp } from "@/lib/format";
import type { MedicalDocument } from "@/types/patients";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select";
import { EmptyState, ErrorState, LoadingState } from "./QueryState";

type Source = "all" | "Patient" | "Doctor";

// Patient mode: the patient's own documents + the ones issued by doctors; uploads patient types.
// Doctor mode: a patient's documents; the doctor uploads prescriptions, orders, reports...
export default function DocumentsPanel(
  props: { mode: "patient" } | { mode: "doctor"; patientId: string; appointmentId?: string },
) {
  const isDoctor = props.mode === "doctor";
  const patientId = isDoctor ? props.patientId : undefined;
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const uploadTypes = isDoctor ? DOCTOR_DOCUMENT_TYPES : PATIENT_DOCUMENT_TYPES;
  const [documentType, setDocumentType] = useState<string>(uploadTypes[0].value);
  const [description, setDescription] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState("");
  const [source, setSource] = useState<Source>("all");

  const queryKey = isDoctor ? ["patient-documents", patientId] : ["documents"];

  const { data: documents, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => (isDoctor ? getPatientDocuments(patientId!) : getMyDocuments()),
  });

  const upload = useMutation({
    mutationFn: () => {
      const input = { file: file!, documentType, description: description.trim() || undefined };
      return isDoctor
        ? uploadPatientDocument(patientId!, { ...input, appointmentId: props.appointmentId })
        : uploadMyDocument(input);
    },
    onSuccess: () => {
      toast.success("Document uploaded");
      queryClient.invalidateQueries({ queryKey });
      setFile(null);
      setDescription("");
      if (fileInputRef.current) fileInputRef.current.value = "";
    },
    onError: (err) => toast.error(err.message),
  });

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const selected = e.target.files?.[0] ?? null;
    setFileError("");
    if (selected && selected.size > MAX_FILE_SIZE) {
      setFileError("The file is larger than 10 MB.");
      setFile(null);
      return;
    }
    if (selected && !ACCEPTED_FILE_TYPES.split(",").includes(selected.type)) {
      setFileError("Only PDF, JPG and PNG files are allowed.");
      setFile(null);
      return;
    }
    setFile(selected);
  }

  const visible = (documents ?? []).filter((d) => source === "all" || d.uploadedByRole === source);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{isDoctor ? "Issue a document" : "Upload a document"}</CardTitle>
          <CardDescription>
            {isDoctor
              ? "Prescriptions, orders, reports, instructions or certificates. The patient will see them in their documents."
              : "Share lab results, imaging, previous exams or reports with your doctors. PDF, JPG or PNG up to 10 MB."}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form
            className="grid gap-4 sm:grid-cols-2"
            onSubmit={(e) => {
              e.preventDefault();
              if (file) upload.mutate();
            }}
          >
            <div className="space-y-2">
              <Label htmlFor="doc-type">Document type</Label>
              <NativeSelect
                id="doc-type"
                value={documentType}
                onChange={(e) => setDocumentType(e.target.value)}
                className="w-full"
              >
                {uploadTypes.map((t) => (
                  <NativeSelectOption key={t.value} value={t.value}>
                    {t.label}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </div>
            <div className="space-y-2">
              <Label htmlFor="doc-file">File</Label>
              <Input
                id="doc-file"
                ref={fileInputRef}
                type="file"
                accept={ACCEPTED_FILE_TYPES}
                onChange={handleFileChange}
              />
              {fileError && <p className="text-xs text-red-500">{fileError}</p>}
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label htmlFor="doc-description">Description (optional)</Label>
              <Input
                id="doc-description"
                maxLength={500}
                placeholder={isDoctor ? "e.g. Complete blood count, fasting" : "e.g. Blood test, March 2026"}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
            <div className="sm:col-span-2">
              <Button type="submit" disabled={!file || upload.isPending}>
                <Upload />
                {upload.isPending ? "Uploading..." : "Upload"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row flex-wrap items-center justify-between gap-3">
          <CardTitle>Documents</CardTitle>
          <NativeSelect
            value={source}
            onChange={(e) => setSource(e.target.value as Source)}
            aria-label="Filter by origin"
          >
            <NativeSelectOption value="all">All documents</NativeSelectOption>
            <NativeSelectOption value="Patient">Shared by the patient</NativeSelectOption>
            <NativeSelectOption value="Doctor">Issued by doctors</NativeSelectOption>
          </NativeSelect>
        </CardHeader>
        <CardContent className="space-y-2">
          {isLoading && <LoadingState label="Loading documents..." />}
          {error && <ErrorState message={error.message} />}
          {documents && visible.length === 0 && (
            <EmptyState icon={FileText} title="No documents yet" />
          )}
          {visible.map((doc) => (
            <DocumentRow key={doc.id} doc={doc} />
          ))}
        </CardContent>
      </Card>
    </div>
  );
}

function DocumentRow({ doc }: { doc: MedicalDocument }) {
  const Icon = doc.fileType.startsWith("image/") ? FileImage : FileText;
  const origin =
    doc.uploadedByRole === "Doctor"
      ? `Issued by ${doc.uploadedByName ? `Dr. ${doc.uploadedByName}` : "a doctor"}`
      : "Shared by the patient";

  return (
    <div className="flex flex-col gap-3 rounded-md border p-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex min-w-0 items-start gap-3">
        <Icon className="mt-0.5 size-5 shrink-0 text-muted-foreground" />
        <div className="min-w-0">
          <p className="truncate text-sm font-medium">{doc.description || doc.fileName}</p>
          <p className="text-xs text-muted-foreground">
            {origin} · {formatTimestamp(doc.uploadedAt)} · {formatFileSize(doc.fileSize)}
          </p>
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Badge variant={doc.uploadedByRole === "Doctor" ? "default" : "outline"}>
          {documentTypeLabel(doc.documentType)}
        </Badge>
        {doc.downloadUrl && (
          <a
            href={doc.downloadUrl}
            target="_blank"
            rel="noreferrer"
            className={buttonVariants({ variant: "ghost", size: "icon-sm" })}
            aria-label={`Download ${doc.fileName}`}
          >
            <Download />
          </a>
        )}
      </div>
    </div>
  );
}
