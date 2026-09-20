"use client";

import Link from "next/link";
import { ArrowRight, LogIn, LogOut, Monitor, Newspaper, Plus } from "lucide-react";
import { Footer, Navigation, Window } from "../page";

function handleLogout() {
  localStorage.removeItem("ludokino_token");
  localStorage.removeItem("ludokino_user");
  window.location.href = "/admin/login";
}

export default function AdminHomePage() {
  const token = typeof window !== "undefined" ? localStorage.getItem("ludokino_token") : null;

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/" />
      <main className="page-shell">
        <Window title="ADMIN" Icon={Monitor}>
          <div className="stack" style={{ gap: 16 }}>
            <p className="article-excerpt">Espace de rédaction rapide pour créer des articles au format markdown.</p>
            <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
              {!token ? (
                <Link className="pixel-button" href="/admin/login">
                  <LogIn size={15} />Connexion
                </Link>
              ) : (
                <button type="button" className="pixel-button" onClick={handleLogout} style={{ background: "#8f1d1d" }}>
                  <LogOut size={15} />Déconnexion
                </button>
              )}
              <Link className="pixel-button" href="/admin/articles">
                <Newspaper size={15} />Articles
              </Link>
              <Link className="pixel-button" href="/admin/articles/new">
                <Plus size={15} />Nouveau brouillon <ArrowRight size={15} />
              </Link>
            </div>
          </div>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
