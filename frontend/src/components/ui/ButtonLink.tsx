import Link from "next/link";
import type { ComponentProps } from "react";
import { cn } from "@/lib/cn";
import { buttonClass, type Variant } from "./Button";

export function ButtonLink({
  variant = "secondary",
  className,
  ...props
}: ComponentProps<typeof Link> & { variant?: Variant }) {
  return <Link className={cn(buttonClass(variant), className)} {...props} />;
}
