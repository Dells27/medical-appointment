import Link from "next/link";
import Brand from "./Brand";

const SECTIONS = [
  {
    title: "Explore",
    links: [
      { href: "/", label: "Home" },
      { href: "/doctors", label: "Doctors" },
      { href: "/specialties", label: "Specialties" },
    ],
  },
  {
    title: "Support",
    links: [{ href: "/contact", label: "Contact" }],
  },
  {
    title: "Legal",
    links: [
      { href: "/privacy", label: "Privacy policy" },
      { href: "/terms", label: "Terms & conditions" },
    ],
  },
];

export default function PublicFooter() {
  return (
    <footer className="border-t bg-muted/40">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 sm:grid-cols-2 lg:grid-cols-4">
        <div className="space-y-3">
          <Brand />
          <p className="text-sm text-muted-foreground">
            Book appointments with trusted doctors and keep your medical history in one place.
          </p>
        </div>

        {SECTIONS.map((section) => (
          <div key={section.title} className="space-y-3">
            <p className="text-sm font-medium">{section.title}</p>
            <ul className="space-y-2 text-sm text-muted-foreground">
              {section.links.map((link) => (
                <li key={link.href}>
                  <Link href={link.href} className="hover:text-foreground">
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
      <div className="border-t">
        <p className="mx-auto max-w-6xl px-4 py-4 text-xs text-muted-foreground">
          © {new Date().getFullYear()} Medical Appointment. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
