"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { registerDoctor, registerPatient } from "@/lib/api/auth";
import { useAppDispatch } from "@/store/hooks";
import { setCredentials } from "@/store/slices/authslices";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface RegisterFormProps {
  role: "Patient" | "Doctor";
}

export default function RegisterForm({ role }: RegisterFormProps) {
  const dispatch = useAppDispatch();
  const router = useRouter();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const title =
    role === "Doctor" ? "Registro de médico" : "Registro de paciente";

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const register = role === "Doctor" ? registerDoctor : registerPatient;
      const data = await register(name, email, password);

      dispatch(
        setCredentials({
          user: { name: data.name, email: data.email, role: data.role },
          token: data.token,
          expiresAt: data.expiresAt,
        }),
      );

      router.push(
        role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard",
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al registrarse");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nombre completo</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                minLength={2}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Contraseña</Label>
              <Input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={8}
              />
            </div>

            {error && <p className="text-sm text-red-500">{error}</p>}

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Creando cuenta..." : "Crear cuenta"}
            </Button>

            <p className="text-center text-sm">
              ¿Ya tenés cuenta?{" "}
              <Link href="/login" className="underline">
                Iniciá sesión
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </main>
  );
}
