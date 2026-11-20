// Simple layout for long legal text
export default function LegalPage({
  title,
  updated,
  sections,
}: {
  title: string;
  updated: string;
  sections: { heading: string; paragraphs: string[] }[];
}) {
  return (
    <article className="mx-auto max-w-3xl space-y-8 px-4 py-12">
      <header className="space-y-2">
        <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
        <p className="text-sm text-muted-foreground">Last updated: {updated}</p>
      </header>

      {sections.map((section, index) => (
        <section key={section.heading} className="space-y-3">
          <h2 className="text-lg font-semibold">
            {index + 1}. {section.heading}
          </h2>
          {section.paragraphs.map((p) => (
            <p key={p.slice(0, 40)} className="leading-7 text-muted-foreground">
              {p}
            </p>
          ))}
        </section>
      ))}
    </article>
  );
}
