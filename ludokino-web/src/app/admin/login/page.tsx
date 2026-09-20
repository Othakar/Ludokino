"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft, LockKeyhole, LogIn } from "lucide-react";
import Link from "next/link";
import { Footer, Navigation, Window } from "../../page";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export default function AdminLoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("admin@ludokino.fr");
  const [password, setPassword] = useState("admin123");
  const [loading, setLoading] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLoading(true);
    setFeedback(null);

    try {
      const response = await fetch(`${apiUrl}/api/Auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      const data = (await response.json().catch(() => null)) as { token?: string; user?: { role?: string } } | null;

      if (!response.ok || !data?.token) {
        throw new Error("Identifiants invalides.");
      }

      localStorage.setItem("ludokino_token", data.token);
      if (data.user) {
        localStorage.setItem("ludokino_user", JSON.stringify(data.user));
      }

      router.push("/admin");
      router.refresh();
    } catch (error) {
      setFeedback(error instanceof Error ? error.message : "Connexion impossible.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/" />
      <main className="page-shell">
        <Link className="text-link" href="/"><ArrowLeft size={16} />Retour au site</Link>
        <Window title="LOGIN ADMIN" Icon={LockKeyhole}>
          <form className="stack" style={{ gap: 18 }} onSubmit={handleSubmit}>
            <p className="article-excerpt">Connectez-vous pour gérer les articles, brouillons et contenus du site.</p>

            <div className="stack" style={{ gap: 10 }}>
              <label className="mono-font editor-label">EMAIL</label>
              <input
                className="editor-input"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="admin@ludokino.fr"
                style={{ minHeight: 48 }}
              />
            </div>

            <div className="stack" style={{ gap: 10 }}>
              <label className="mono-font editor-label">MOT DE PASSE</label>
              <input
                className="editor-input"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="••••••••"
                style={{ minHeight: 48 }}
              />
            </div>

            {feedback && (
              <p className="article-excerpt" style={{ color: "#ffe8a3" }}>{feedback}</p>
            )}

            <button type="submit" className="pixel-button" disabled={loading} style={{ width: "max-content" }}>
              <LogIn size={15} />{loading ? "Connexion..." : "Se connecter"}
            </button>
          </form>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
