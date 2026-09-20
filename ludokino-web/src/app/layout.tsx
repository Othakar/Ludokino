import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "LUDOKINO - Culture geek, anime et jeux vidéo",
  description: "Le média Ludokino : émissions, articles et culture geek.",
  icons: {
    icon: "/img/LDKNfavicon.png",
  },
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="fr">
      <body>{children}</body>
    </html>
  );
}
