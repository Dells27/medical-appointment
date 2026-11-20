import Link from "next/link";

// Simpler footer for signed-in users
export default function AppFooter() {
  return (
    <footer className="border-t">
      <div className="flex flex-col gap-2 px-4 py-4 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between md:px-6">
        <p>© {new Date().getFullYear()} Medical Appointment</p>
        <nav className="flex gap-4">
          <Link href="/contact" className="hover:text-foreground">
            Support
          </Link>
          <Link href="/privacy" className="hover:text-foreground">
            Privacy
          </Link>
          <Link href="/terms" className="hover:text-foreground">
            Terms
          </Link>
        </nav>
      </div>
    </footer>
  );
}
