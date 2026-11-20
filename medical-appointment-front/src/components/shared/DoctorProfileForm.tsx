"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { createDoctorProfile, updateMyDoctorProfile } from "@/lib/api/doctors";
import type { Doctor } from "@/types/doctor";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

// Creates the doctor profile, or edits it when `profile` is given.
// Email and license number cannot be changed after creation.
export default function DoctorProfileForm({ profile }: { profile?: Doctor }) {
  const user = useAppSelector((state) => state.auth.user);
  const queryClient = useQueryClient();
  const isEdit = !!profile;

  const [name, setName] = useState(profile?.name ?? user?.name ?? "");
  const [specialty, setSpecialty] = useState(profile?.specialty ?? "");
  const [licenseNumber, setLicenseNumber] = useState(profile?.licenseNumber ?? "");
  const [bio, setBio] = useState(profile?.bio ?? "");
  const [photoUrl, setPhotoUrl] = useState(profile?.photoUrl ?? "");

  const mutation = useMutation({
    mutationFn: () =>
      isEdit
        ? updateMyDoctorProfile({
            name: name.trim(),
            specialty: specialty.trim(),
            bio: bio.trim() || undefined,
            photoUrl: photoUrl.trim() || undefined,
          })
        : createDoctorProfile({
            name: name.trim(),
            email: user!.email,
            specialty: specialty.trim(),
            licenseNumber: licenseNumber.trim(),
          }),
    onSuccess: (saved) => {
      toast.success(isEdit ? "Profile updated" : "Profile created");
      queryClient.setQueryData(["doctor-profile"], saved);
      queryClient.invalidateQueries({ queryKey: ["public-doctors"] });
      queryClient.invalidateQueries({ queryKey: ["specialties"] });
    },
  });

  function handleSubmit(e: React.SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    mutation.mutate();
  }

  return (
    <Card className={isEdit ? "" : "mx-auto max-w-lg"}>
      <CardHeader>
        <CardTitle>{isEdit ? "Professional profile" : "Complete your profile"}</CardTitle>
        <CardDescription>
          {isEdit
            ? "Patients see this information when they look for a doctor."
            : "Patients will find you in the doctor directory once your profile is created."}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="name">Full name</Label>
            <Input id="name" value={name} onChange={(e) => setName(e.target.value)} required minLength={2} />
          </div>
          <div className="space-y-2">
            <Label>Email</Label>
            <Input value={profile?.email ?? user?.email ?? ""} disabled />
          </div>
          <div className="space-y-2">
            <Label htmlFor="specialty">Specialty</Label>
            <Input
              id="specialty"
              placeholder="e.g. Cardiology"
              value={specialty}
              onChange={(e) => setSpecialty(e.target.value)}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="license">License number</Label>
            <Input
              id="license"
              value={licenseNumber}
              onChange={(e) => setLicenseNumber(e.target.value)}
              disabled={isEdit}
              required
            />
          </div>

          {isEdit && (
            <>
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="bio">About you</Label>
                <Textarea
                  id="bio"
                  rows={4}
                  placeholder="Experience, approach, languages spoken..."
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                />
              </div>
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="photo">Photo URL (optional)</Label>
                <Input
                  id="photo"
                  type="url"
                  placeholder="https://..."
                  value={photoUrl}
                  onChange={(e) => setPhotoUrl(e.target.value)}
                />
              </div>
            </>
          )}

          {mutation.error && (
            <p className="text-sm text-red-500 sm:col-span-2">{mutation.error.message}</p>
          )}

          <div className="sm:col-span-2">
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Saving..." : isEdit ? "Save changes" : "Create profile"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
