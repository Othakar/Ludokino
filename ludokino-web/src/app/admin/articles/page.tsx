"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowLeft, LogOut, Pencil, Plus, Trash2 } from "lucide-react";
import { Footer, Navigation, Window } from "../../page";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type ArticleItem = {
  id: number;
  slug: string;
  title: string;
  excerpt: string;
  status?: string;
  publishedAt?: string | null;
  categories?: string[];
};

function formatDate(value?: string | null) {
  if (!value) return "—";
  return new Intl.DateTimeFormat("fr-FR", { dateStyle: "medium" }).format(new Date(value));
}

function handleLogout() {
  localStorage.removeItem("ludokino_token");
  localStorage.removeItem("ludokino_user");
  window.location.href = "/admin/login";
}

export default function AdminArticlesPage() {
  const [articles, setArticles] = useState<ArticleItem[]>([]);
  const [token, setToken] = useState("");
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<"all" | "Draft" | "Published">("all");

  useEffect(() => {
    const savedToken = localStorage.getItem("ludokino_token") ?? "";
    setToken(savedToken);

    const loadArticles = async () => {
      try {
        const publishedResponse = await fetch(`${apiUrl}/api/Articles?page=1&pageSize=100`);
        const published = publishedResponse.ok ? ((await publishedResponse.json()) as ArticleItem[]) : [];

        let drafts: ArticleItem[] = [];
        if (savedToken.trim()) {
          const draftResponse = await fetch(`${apiUrl}/api/Articles/drafts`, {
            headers: { Authorization: `Bearer ${savedToken.trim()}` },
          });
          drafts = draftResponse.ok ? ((await draftResponse.json()) as ArticleItem[]) : [];
        }

        const merged = [...published, ...drafts].filter((article, index, list) => {
          const first = list.findIndex((item) => item.id === article.id);
          return first === index;
        });

        setArticles(merged.sort((left, right) => right.id - left.id));
      } catch {
        setArticles([]);
      } finally {
        setLoading(false);
      }
    };

    void loadArticles();
  }, []);

  const filteredArticles = useMemo(() => {
    if (filter === "all") return articles;
    return articles.filter((article) => (article.status ?? "Published") === filter);
  }, [articles, filter]);

  const handleDelete = async (articleId: number) => {
    if (!token.trim()) {
      return;
    }

    const confirmed = window.confirm("Supprimer cet article ?");
    if (!confirmed) return;

    try {
      const response = await fetch(`${apiUrl}/api/Articles/${articleId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token.trim()}` },
      });

      if (response.ok) {
        setArticles((current) => current.filter((article) => article.id !== articleId));
      }
    } catch {
      alert("Suppression impossible.");
    }
  };

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/blog" />
      <main className="page-shell">
        <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
          <Link className="text-link" href="/admin"><ArrowLeft size={16} />Retour à l&apos;admin</Link>
          <button type="button" className="pixel-button" onClick={handleLogout} style={{ background: "#8f1d1d" }}>
            <LogOut size={15} />Déconnexion
          </button>
        </div>
        <Window title="ARTICLES" Icon={Plus}>
          <div className="stack" style={{ gap: 20 }}>
            <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
              <p className="article-excerpt">Gérez vos contenus et brouillons.</p>
              <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                <button type="button" className={`filter-chip ${filter === "all" ? "active" : ""}`} onClick={() => setFilter("all")}>Tous</button>
                <button type="button" className={`filter-chip ${filter === "Draft" ? "active" : ""}`} onClick={() => setFilter("Draft")}>Brouillons</button>
                <button type="button" className={`filter-chip ${filter === "Published" ? "active" : ""}`} onClick={() => setFilter("Published")}>Publiés</button>
              </div>
            </div>

            <div style={{ display: "flex", justifyContent: "flex-end" }}>
              <Link className="pixel-button" href="/admin/articles/new">Nouveau <Plus size={15} /></Link>
            </div>

            {loading ? (
              <p className="article-excerpt">Chargement…</p>
            ) : filteredArticles.length === 0 ? (
              <p className="article-excerpt">Aucun article pour ce filtre.</p>
            ) : (
              <div className="stack" style={{ gap: 12 }}>
                {filteredArticles.map((article) => (
                  <div className="window" key={article.id}>
                    <div className="window-body" style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap", alignItems: "center", padding: "18px 16px" }}>
                      <div className="stack" style={{ gap: 4 }}>
                        <span className="mono-font" style={{ color: "#f7c92f", fontSize: "0.72rem" }}>{article.status ?? "Published"}</span>
                        <h2 className="show-card-title" style={{ textTransform: "none" }}>{article.title}</h2>
                        <p className="article-excerpt">{article.excerpt || "Aucun extrait."}</p>
                        <div className="article-taxonomy">
                          {(article.categories ?? []).map((category) => (
                            <span key={`${article.id}-${category}`}>{category}</span>
                          ))}
                        </div>
                        <span className="mono-font" style={{ color: "#c0cfe3", fontSize: "0.7rem" }}>{formatDate(article.publishedAt)}</span>
                      </div>

                      <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                        <Link className="pixel-button" href={`/admin/articles/${article.id}`}>
                          <Pencil size={15} />Éditer
                        </Link>
                        <button type="button" className="pixel-button" onClick={() => handleDelete(article.id)} style={{ background: "#8f1d1d" }}>
                          <Trash2 size={15} />Supprimer
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
