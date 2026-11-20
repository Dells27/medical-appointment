"use client";

import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import type { MonthlyCount } from "@/types/appointment";

// One series, one hue (reference categorical slot 1), validated against the light and dark
// surfaces with the dataviz palette validator. A single series needs no legend: the card title names it.
const chartConfig = {
  completed: {
    label: "Completed visits",
    theme: { light: "#2a78d6", dark: "#3987e5" },
  },
} satisfies ChartConfig;

function monthLabel(month: string) {
  const [y, m] = month.split("-").map(Number);
  return new Date(y, m - 1, 1).toLocaleDateString("en-US", { month: "short" });
}

export default function CompletedVisitsChart({ data }: { data: MonthlyCount[] }) {
  const rows = data.map((d) => ({ ...d, label: monthLabel(d.month) }));

  return (
    <>
      <ChartContainer config={chartConfig} className="aspect-auto h-56 w-full">
        <BarChart data={rows} margin={{ top: 8, right: 8, left: -16, bottom: 0 }} barCategoryGap="30%">
          <CartesianGrid vertical={false} strokeDasharray="3 3" strokeOpacity={0.5} />
          <XAxis dataKey="label" tickLine={false} axisLine={false} tickMargin={8} />
          <YAxis allowDecimals={false} tickLine={false} axisLine={false} width={40} />
          <ChartTooltip cursor={{ fillOpacity: 0.08 }} content={<ChartTooltipContent />} />
          <Bar dataKey="completed" fill="var(--color-completed)" radius={[4, 4, 0, 0]} maxBarSize={40} />
        </BarChart>
      </ChartContainer>

      {/* Same numbers as a table, for screen readers */}
      <table className="sr-only">
        <caption>Completed visits per month</caption>
        <thead>
          <tr>
            <th scope="col">Month</th>
            <th scope="col">Completed</th>
            <th scope="col">Cancelled</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.month}>
              <th scope="row">{r.label}</th>
              <td>{r.completed}</td>
              <td>{r.cancelled}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
