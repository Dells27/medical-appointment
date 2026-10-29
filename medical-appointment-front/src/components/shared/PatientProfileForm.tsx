"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useAppSelector } from "@/store/hooks";
import { createPatientProfile } from "@/lib/api/patients";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

const BLOOD_TYPES = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

export default function PatientProfileForm() {
  const token = useAppSelector((state) => state.auth.token);
  const user = useAppSelector((state) => state.auth.user);
  const queryClient = useQueryClient();

  const [dateOfBirth, setDateOfBirth] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [bloodType, setBloodType] = useState("");
  const [allergies, setAllergies] = useState("");

  const today = new Date().toLocaleDateString("en-CA");

  const mutation = useMutation({
    mutationFn: () =>
      createPatientProfile(token!, {
        fullName: user!.name,
        email: user!.email,
        dateOfBirth,
        phoneNumber,
        bloodType,
        allergies: allergies || undefined,
      }),
    onSuccess: () => {
      toast.success("Profile created");
      queryClient.invalidateQueries({ queryKey: ["patient-profile"] });
    },
  });

  function handleSubmit(e: React.SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    mutation.mutate();
  }

  return (
    <Card className="mx-auto max-w-md">
      <CardHeader>
        <CardTitle>Complete your profile</CardTitle>
        <CardDescription>
          This information helps your doctors take better care of you.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="dob">Date of birth</Label>
            <Input
              id="dob"
              type="date"
              max={today}
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
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2">
            <Label>Blood type</Label>
            <div className="grid grid-cols-4 gap-2">
              {BLOOD_TYPES.map((type) => (
                <Button
                  key={type}
                  type="button"
                  size="sm"
                  variant={bloodType === type ? "default" : "outline"}
                  onClick={() => setBloodType(type)}
                >
                  {type}
                </Button>
              ))}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="allergies">Allergies (optional)</Label>
            <Input
              id="allergies"
              value={allergies}
              onChange={(e) => setAllergies(e.target.value)}
            />
          </div>

          {mutation.error && <p className="text-sm text-red-500">{mutation.error.message}</p>}

          <Button
            type="submit"
            className="w-full"
            disabled={!bloodType || mutation.isPending}
          >
            {mutation.isPending ? "Saving..." : "Create profile"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}