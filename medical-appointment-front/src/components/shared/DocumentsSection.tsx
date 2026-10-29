"use client";

import { useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { getDocuments, uploadDocument } from "@/lib/api/patients";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const DOCUMENT_TYPES = ["Exam", "X-ray", "Prescription", "Other"];

export default function DocumentsSection() {
  const token = useAppSelector((state) => state.auth.token);
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [documentType, setDocumentType] = useState("Exam");

  const { data: documents, isLoading, error } = useQuery({
    queryKey: ["documents"],
    queryFn: () => getDocuments(token!),
    enabled: !!token,
  });

  const upload = useMutation({
    mutationFn: (file: File) => uploadDocument(token!, file, documentType),
    onSuccess: () => {
      toast.success("Document uploaded");
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
    onError: (err) => toast.error(err.message),
  });

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) upload.mutate(file);
    e.target.value = ""; // permite volver a elegir el mismo archivo
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Medical documents</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-3">
          <Label>Document type</Label>
          <div className="flex flex-wrap gap-2">
            {DOCUMENT_TYPES.map((type) => (
              <Button
                key={type}
                type="button"
                size="sm"
                variant={documentType === type ? "default" : "outline"}
                onClick={() => setDocumentType(type)}
              >
                {type}
              </Button>
            ))}
          </div>

          <input
            ref={fileInputRef}
            type="file"
            className="hidden"
            onChange={handleFileChange}
          />
          <Button
            onClick={() => fileInputRef.current?.click()}
            disabled={upload.isPending}
          >
            {upload.isPending ? "Uploading..." : "Upload file"}
          </Button>
        </div>

        {isLoading && <p>Loading documents...</p>}
        {error && <p className="text-red-500">Could not load documents.</p>}
        {documents?.length === 0 && (
          <p className="text-sm text-muted-foreground">No documents uploaded yet.</p>
        )}

        <div className="space-y-2">
          {documents?.map((doc) => (
            <div
              key={doc.id}
              className="flex items-center justify-between gap-4 rounded-md border p-3"
            >
              <div className="min-w-0">
                <p className="truncate text-sm font-medium">{doc.fileName}</p>
                <p className="text-xs text-muted-foreground">
                  {doc.uploadedAt.slice(0, 10)} · {Math.max(1, Math.round(doc.fileSize / 1024))} KB
                </p>
              </div>
              <div className="flex items-center gap-3">
                <Badge variant="outline">{doc.documentType}</Badge>
                {doc.downloadUrl && (
                  <a
                    href={doc.downloadUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="text-sm underline"
                  >
                    Download
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}