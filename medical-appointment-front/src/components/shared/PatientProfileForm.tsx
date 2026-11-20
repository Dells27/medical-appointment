"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { createPatientProfile, updatePatientProfile } from "@/lib/api/patients";
import { dateKey, todayKey } from "@/lib/format";
import type { PatientProfile } from "@/types/patients";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

const BLOOD_TYPES = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

// Creates the patient profile, or edits it when `profile` is given
export default function PatientProfileForm({
  profile,
  onDone,
}: {
  profile?: PatientProfile;
  onDone?: () => void;
}) {
  const user = useAppSelector((state) => state.auth.user);
  const queryClient = useQueryClient();
  const isEdit = !!profile;

  const [name, setName] = useState(profile?.name ?? user?.name ?? "");
  const [dateOfBirth, setDateOfBirth] = useState(profile ? dateKey(profile.dateOfBirth) : "");
  const [phoneNumber, setPhoneNumber] = useState(profile?.phoneNumber ?? "");
  const [bloodType, setBloodType] = useState(profile?.bloodType ?? "");
  const [allergies, setAllergies] = useState(profile?.allergies ?? "");

  const mutation = useMutation({
    mutationFn: () => {
      const data = {
        name: name.trim(),
        dateOfBirth,
        phoneNumber: phoneNumber.trim(),
        bloodType,
        allergies: allergies.trim() || undefined,
      };
      return isEdit
        ? updatePatientProfile(data)
        : createPatientProfile({ ...data, email: user!.email });
    },
    onSuccess: (saved) => {
      toast.success(isEdit ? "Profile updated" : "Profile created");
      queryClient.setQueryData(["patient-profile"], saved);
      onDone?.();
    },
  });

  function handleSubmit(e: React.SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    mutation.mutate();
  }

  return (
    <Card className={isEdit ? "" : "mx-auto max-w-lg"}>
      <CardHeader>
        <CardTitle>{isEdit ? "Edit personal information" : "Complete your profile"}</CardTitle>
        {!isEdit && (
          <CardDescription>
            This information helps your doctors take better care of you.
          </CardDescription>
        )}
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2 sm:col-span-2">
            <Label htmlFor="name">Full name</Label>
            <Input id="name" value={name} onChange={(e) => setName(e.target.value)} required minLength={2} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="dob">Date of birth</Label>
            <Input
              id="dob"
              type="date"
              max={todayKey()}
              value={dateOfBirth}
              onChange={(e) => setDateOfBirth(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="phone">Phone number</Label>
            <Input
              id="phone"
              type="tel"
              autoComplete="tel"
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2 sm:col-span-2">
            <Label>Blood type</Label>
            <div className="grid grid-cols-4 gap-2 sm:grid-cols-8">
              {BLOOD_TYPES.map((type) => (
                <Button
                  key={type}
                  type="button"
                  size="sm"
                  variant={bloodType === type ? "default" : "outline"}
                  onClick={() => setBloodType(type)}
                  aria-pressed={bloodType === type}
                >
                  {type}
                </Button>
              ))}
            </div>
          </div>

          <div className="space-y-2 sm:col-span-2">
            <Label htmlFor="allergies">Known allergies (optional)</Label>
            <Textarea
              id="allergies"
              placeholder="e.g. Penicillin, peanuts"
              value={allergies}
              onChange={(e) => setAllergies(e.target.value)}
            />
          </div>

          {mutation.error && (
            <p className="text-sm text-red-500 sm:col-span-2">{mutation.error.message}</p>
          )}

          <div className="flex gap-2 sm:col-span-2">
            <Button type="submit" disabled={!bloodType || mutation.isPending}>
              {mutation.isPending ? "Saving..." : isEdit ? "Save changes" : "Create profile"}
            </Button>
            {isEdit && onDone && (
              <Button type="button" variant="outline" onClick={onDone}>
                Cancel
              </Button>
            )}
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
