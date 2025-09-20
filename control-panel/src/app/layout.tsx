import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "MathQuest Teacher Console",
  description:
    "Admin control panel for monitoring MathQuest Escape student progress and adjusting rewards.",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-slate-100">
        <div className="min-h-screen">
          {children}
        </div>
      </body>
    </html>
  );
}
