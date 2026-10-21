"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { login } from "@/lib/api/auth";
import { useAppDispatch } from "@/store/hooks";
import { setCredentials } from "@/store/slices/authslices";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import Link from "next/link";

export default function LoginPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const data = await login(email, password);

      dispatch(
        setCredentials({
          user: { name: data.name, email: data.email, role: data.role },
          token: data.token,
          expiresAt: data.expiresAt,
        }),
      );

      router.push(
        data.role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard",
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al iniciar sesión");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Iniciar sesión</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
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
              />
            </div>

            {error && <p className="text-sm text-red-500">{error}</p>}

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Entrando..." : "Entrar"}
            </Button>
            <p className="text-center text-sm">
              ¿No tenés cuenta?{" "}
              <Link href="/register/patient" className="underline">
                Soy paciente
              </Link>
              {" · "}
              <Link href="/register/doctor" className="underline">
                Soy médico
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </main>
  );
}
