"use client";

import { useEffect, useState } from "react";
import { ArrowLeft, Monitor } from "lucide-react";
import Link from "next/link";
import { Footer, Navigation, Window } from "../../../page";
import ArticleAdminForm from "@/components/ArticleAdminForm";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type ArticleDetail = {
  id: number;
  title: string;
  excerpt: string;
  content: string;
  coverImageUrl?: string | null;
  videoUrl?: string | null;
  status: string;
  categories?: { id: number; name: string; slug: string }[];
  tags?: { id: number; name: string; slug: string }[];
};

export default function EditArticlePage({ params }: { params: Promise<{ id: string }> }) {
  const [article, setArticle] = useState<ArticleDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const resolved = await params;
        const token = localStorage.getItem("ludokino_token") ?? "";
        const response = await fetch(`${apiUrl}/api/Articles/id/${Number(resolved.id)}`, {
          headers: token.trim() ? { Authorization: `Bearer ${token.trim()}` } : undefined,
        });
        if (!response.ok) {
          setArticle(null);
          return;
        }

        const data = (await response.json()) as ArticleDetail;
        setArticle(data);
      } catch {
        setArticle(null);
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params]);

  if (loading) {
    return (
      <div>
        <div className="scanline" />
        <div className="crt-overlay" aria-hidden="true" />
        <Navigation activeHref="/blog" />
        <main className="page-shell"><p className="shows-empty">Chargement…</p></main>
        <Footer />
      </div>
    );
  }

  if (!article) {
    return (
      <div>
        <div className="scanline" />
        <div className="crt-overlay" aria-hidden="true" />
        <Navigation activeHref="/blog" />
        <main className="page-shell"><p className="shows-empty">Article introuvable.</p></main>
        <Footer />
      </div>
    );
  }

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/blog" />
      <main className="page-shell">
        <Link className="text-link" href="/admin/articles"><ArrowLeft size={16} />Retour à la liste</Link>
        <Window title="ÉDITION ARTICLE" Icon={Monitor}>
          <ArticleAdminForm
            mode="edit"
            articleId={article.id}
            initialArticle={{
              id: article.id,
              title: article.title,
              excerpt: article.excerpt,
              content: article.content,
              coverImageUrl: article.coverImageUrl,
              videoUrl: article.videoUrl,
              status: article.status,
              categoryIds: article.categories?.map((category) => category.id) ?? [],
              tagIds: article.tags?.map((tag) => tag.id) ?? [],
            }}
          />
        </Window>
      </main>
      <Footer />
    </div>
  );
}
