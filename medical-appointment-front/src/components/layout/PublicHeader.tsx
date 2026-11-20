"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Menu } from "lucide-react";
import { useAppSelector } from "@/store/hooks";
import { cn } from "@/lib/utils";
import { buttonVariants } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import Brand from "./Brand";

const LINKS = [
  { href: "/doctors", label: "Doctors" },
  { href: "/specialties", label: "Specialties" },
  { href: "/contact", label: "Contact" },
];

export default function PublicHeader() {
  const pathname = usePathname();
  const { user, isAuthenticated } = useAppSelector((state) => state.auth);
  const [open, setOpen] = useState(false);

  const dashboard = user?.role === "Doctor" ? "/doctor/dashboard" : "/patient/dashboard";

  const authActions = isAuthenticated ? (
    <Link href={dashboard} className={buttonVariants({ size: "sm" })}>
      Go to dashboard
    </Link>
  ) : (
    <>
      <Link href="/login" className={buttonVariants({ variant: "ghost", size: "sm" })}>
        Sign in
      </Link>
      <Link href="/register/patient" className={buttonVariants({ size: "sm" })}>
        Get started
      </Link>
    </>
  );

  return (
    <header className="sticky top-0 z-30 border-b bg-background/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Brand />

        <nav className="hidden items-center gap-6 text-sm md:flex">
          {LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={cn(
                "text-muted-foreground transition-colors hover:text-foreground",
                pathname.startsWith(link.href) && "font-medium text-foreground",
              )}
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="hidden items-center gap-2 md:flex">{authActions}</div>

        {/* Mobile menu */}
        <Sheet open={open} onOpenChange={setOpen}>
          <SheetTrigger
            className={cn(buttonVariants({ variant: "ghost", size: "icon" }), "md:hidden")}
            aria-label="Open menu"
          >
            <Menu />
          </SheetTrigger>
          <SheetContent side="right" className="w-72">
            <SheetHeader>
              <SheetTitle>Menu</SheetTitle>
            </SheetHeader>
            <nav className="flex flex-col gap-1 px-4" onClick={() => setOpen(false)}>
              <Link href="/" className="rounded-md px-2 py-2 hover:bg-muted">
                Home
              </Link>
              {LINKS.map((link) => (
                <Link key={link.href} href={link.href} className="rounded-md px-2 py-2 hover:bg-muted">
                  {link.label}
                </Link>
              ))}
              <div className="mt-4 flex flex-col gap-2">{authActions}</div>
            </nav>
          </SheetContent>
        </Sheet>
      </div>
    </header>
  );
}
