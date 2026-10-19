import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { headers } from "next/headers";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export async function generateMetadata(): Promise<Metadata> {
  const requestHeaders = await headers();
  const host = requestHeaders.get("x-forwarded-host") ?? requestHeaders.get("host") ?? "icarus-control.local";
  const protocol = requestHeaders.get("x-forwarded-proto") ?? (host.startsWith("localhost") ? "http" : "https");
  const origin = `${protocol}://${host}`;

  return {
    metadataBase: new URL(origin),
    title: "Icarus Control",
    description: "Veilige operations-console voor de Icarus-gameserver.",
    openGraph: {
      title: "Icarus Control",
      description: "Gebruikers, netwerktoegang, kaartpolicies en releases in één veilige game-operations-console.",
      type: "website",
      url: origin,
      images: [{ url: `${origin}/og.png`, width: 1745, height: 909, alt: "Icarus Control game operations" }],
    },
    twitter: {
      card: "summary_large_image",
      title: "Icarus Control",
      description: "Veilige game operations voor Project Icarus.",
      images: [`${origin}/og.png`],
    },
  };
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="nl">
      <body className={`${geistSans.variable} ${geistMono.variable}`}>
        {children}
      </body>
    </html>
  );
}
