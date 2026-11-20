"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { LogOut, Stethoscope } from "lucide-react";
import { useAppSelector } from "@/store/hooks";
import { useLogout } from "@/hooks/use-logout";
import { initials } from "@/lib/format";
import type { Role } from "@/types/auth";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
  useSidebar,
} from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";
import AppFooter from "./AppFooter";
import { NAV_ITEMS } from "./nav-items";

function AppSidebar({ role }: { role: Role }) {
  const pathname = usePathname();
  const user = useAppSelector((state) => state.auth.user);
  const handleLogout = useLogout();
  const { isMobile, setOpenMobile } = useSidebar();

  // On mobile the sidebar is a drawer: close it after navigating
  const closeOnMobile = () => {
    if (isMobile) setOpenMobile(false);
  };

  return (
    <Sidebar>
      <SidebarHeader>
        <Link
          href={NAV_ITEMS[role][0].href}
          onClick={closeOnMobile}
          className="flex items-center gap-2 px-2 py-1.5 font-semibold"
        >
          <span className="flex size-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <Stethoscope className="size-4" />
          </span>
          Medical Appointment
        </Link>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {NAV_ITEMS[role].map((item) => {
                const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      isActive={active}
                      render={<Link href={item.href} onClick={closeOnMobile} />}
                    >
                      <item.icon />
                      <span>{item.label}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <div className="flex items-center gap-2 px-2 py-1.5">
          <span className="flex size-8 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium">
            {initials(user?.name)}
          </span>
          <div className="min-w-0 text-sm">
            <p className="truncate font-medium">{user?.name}</p>
            <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
          </div>
        </div>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton onClick={handleLogout}>
              <LogOut />
              <span>Sign out</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}

function CurrentPageTitle({ role }: { role: Role }) {
  const pathname = usePathname();
  const item = NAV_ITEMS[role].find(
    (i) => pathname === i.href || pathname.startsWith(`${i.href}/`),
  );
  return <span className="text-sm font-medium">{item?.label}</span>;
}

// Authenticated layout: collapsible sidebar, top bar and simple footer
export default function AppShell({ role, children }: { role: Role; children: React.ReactNode }) {
  return (
    <SidebarProvider>
      <AppSidebar role={role} />
      <SidebarInset>
        <header className="sticky top-0 z-20 flex h-14 items-center gap-2 border-b bg-background/90 px-4 backdrop-blur">
          <SidebarTrigger />
          <Separator orientation="vertical" className="h-4" />
          <CurrentPageTitle role={role} />
        </header>
        <div className="flex-1">
          <div className="mx-auto w-full max-w-6xl space-y-6 p-4 md:p-6">{children}</div>
        </div>
        <AppFooter />
      </SidebarInset>
    </SidebarProvider>
  );
}
