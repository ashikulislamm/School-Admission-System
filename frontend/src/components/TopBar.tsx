"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { StudentIdField } from "@/components/StudentIdField";
import { cn } from "@/lib/cn";

const NAV = [
  { href: "/", label: "Classes" },
  { href: "/applications", label: "Applications" },
  { href: "/admin", label: "Admin" },
] as const;

function isActive(pathname: string, href: string): boolean {
  return href === "/" ? pathname === "/" : pathname === href || pathname.startsWith(`${href}/`);
}

export function TopBar() {
  const pathname = usePathname();

  return (
    <header className="border-b border-line bg-surface">
      <div className="mx-auto flex min-h-14 w-full max-w-[1040px] flex-wrap items-center gap-x-6 px-4 sm:px-6">
        <Link href="/" className="text-section text-fg">
          Admissions
        </Link>
        <nav aria-label="Main" className="-mb-px flex h-14 max-sm:order-last max-sm:w-full max-sm:overflow-x-auto sm:flex-1">
          {NAV.map(({ href, label }) => {
            const active = isActive(pathname, href);
            return (
              <Link
                key={href}
                href={href}
                aria-current={active ? "page" : undefined}
                className={cn(
                  "flex h-full items-center border-b-2 px-3 text-body font-medium whitespace-nowrap",
                  active ? "border-accent text-fg" : "border-transparent text-muted hover:text-fg",
                )}
              >
                {label}
              </Link>
            );
          })}
        </nav>
        <div className="ml-auto max-sm:py-2">
          <StudentIdField />
        </div>
      </div>
    </header>
  );
}
