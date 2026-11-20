import Link from "next/link";
import { Compass } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import PublicHeader from "@/components/layout/PublicHeader";
import PublicFooter from "@/components/layout/PublicFooter";

export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col">
      <PublicHeader />
      <main className="flex flex-1 flex-col items-center justify-center gap-4 px-4 py-24 text-center">
        <Compass className="size-12 text-primary" />
        <p className="text-sm font-medium text-primary">404</p>
        <h1 className="text-3xl font-semibold tracking-tight">Page not found</h1>
        <p className="max-w-md text-muted-foreground">
          The page you are looking for doesn&apos;t exist or was moved.
        </p>
        <div className="flex gap-3 pt-2">
          <Link href="/" className={buttonVariants()}>
            Go home
          </Link>
          <Link href="/doctors" className={buttonVariants({ variant: "outline" })}>
            Browse doctors
          </Link>
        </div>
      </main>
      <PublicFooter />
    </div>
  );
}
