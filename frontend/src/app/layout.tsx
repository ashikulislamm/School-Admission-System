import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { connection } from "next/server";
import { TopBar } from "@/components/TopBar";
import { Providers } from "./providers";
import "./globals.css";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Admissions",
};

export default async function RootLayout({ children }: LayoutProps<"/">) {
  // Opt into request-time rendering so BACKEND_URL is read when the container runs, not at build.
  await connection();
  const config = { BACKEND_URL: process.env.BACKEND_URL ?? "" };
  // "<" is escaped so the value can never close the script tag.
  const script = `window.__CONFIG__=${JSON.stringify(config).replace(/</g, "\\u003c")};`;

  return (
    <html lang="en" className={inter.variable}>
      <head>
        <script dangerouslySetInnerHTML={{ __html: script }} />
      </head>
      <body className="min-h-screen bg-bg font-sans text-body text-fg">
        <Providers>
          <TopBar />
          <main className="mx-auto w-full max-w-[1040px] px-4 py-6 sm:px-6">{children}</main>
        </Providers>
      </body>
    </html>
  );
}
