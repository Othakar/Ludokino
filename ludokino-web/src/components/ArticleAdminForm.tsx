"use client";

import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { ArrowLeft, CheckCircle2, Save } from "lucide-react";
import Link from "next/link";
import MarkdownEditor from "@/components/MarkdownEditor";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type Category = { id: number; name: string; slug: string };
type Tag = { id: number; name: string; slug: string };
type ArticlePayload = {
  id: number;
  title: string;
  excerpt: string;
  content: string;
  coverImageUrl?: string | null;
  videoUrl?: string | null;
  status: string;
  categoryIds?: number[];
  tagIds?: number[];
};

type ArticleAdminFormProps = {
  mode: "create" | "edit";
  articleId?: number;
  initialArticle?: Partial<ArticlePayload> | null;
};

function normalizeUrls(value: string) {
  return value
    .split(/\n|,|;/)
    .map((part) => part.trim())
    .filter(Boolean)
    .filter((part, index, array) => array.indexOf(part) === index);
}

export default function ArticleAdminForm({ mode, articleId, initialArticle }: ArticleAdminFormProps) {
  const [token, setToken] = useState("");
  const [categories, setCategories] = useState<Category[]>([]);
  const [tags, setTags] = useState<Tag[]>([]);
  const [isBusy, setIsBusy] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);
  const [selectedCategories, setSelectedCategories] = useState<number[]>([]);
  const [selectedTags, setSelectedTags] = useState<number[]>([]);
  const [form, setForm] = useState({
    title: "",
    excerpt: "",
    content: "# Titre\n\n## Sous-titre\n\nVotre article commence ici.\n\n- point 1\n- point 2\n\n[Voir le lien](https://example.com)\n\n![Image](https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80)",
    coverImageUrl: "",
    videoUrl: "",
    status: "Draft",
  });

  useEffect(() => {
    const savedToken = localStorage.getItem("ludokino_token") ?? "";
    setToken(savedToken);

    const loadCatalog = async () => {
      try {
        const [categoryResponse, tagResponse] = await Promise.all([
          fetch(`${apiUrl}/api/Categories`),
          fetch(`${apiUrl}/api/Tags`),
        ]);

        if (categoryResponse.ok) {
          const categoryData = (await categoryResponse.json()) as Category[];
          setCategories(Array.isArray(categoryData) ? categoryData : []);
        }

        if (tagResponse.ok) {
          const tagData = (await tagResponse.json()) as Tag[];
          setTags(Array.isArray(tagData) ? tagData : []);
        }
      } catch {
        setFeedback("Impossible de charger les catégories et tags.");
      }
    };

    void loadCatalog();
  }, []);

  useEffect(() => {
    if (!initialArticle) {
      return;
    }

    setForm({
      title: initialArticle.title ?? "",
      excerpt: initialArticle.excerpt ?? "",
      content: initialArticle.content ?? "",
      coverImageUrl: initialArticle.coverImageUrl ?? "",
      videoUrl: initialArticle.videoUrl ?? "",
      status: initialArticle.status ?? "Draft",
    });
    setSelectedCategories(initialArticle.categoryIds ?? []);
    setSelectedTags(initialArticle.tagIds ?? []);
  }, [initialArticle]);

  const canSubmit = useMemo(
    () => form.title.trim().length > 2 && form.content.trim().length > 0 && token.trim().length > 0,
    [form.content, form.title, token],
  );

  const handleTokenSave = () => {
    localStorage.setItem("ludokino_token", token.trim());
    setFeedback("Jeton enregistré dans le navigateur.");
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!canSubmit) {
      setFeedback("Le titre, le contenu et le jeton sont requis.");
      return;
    }

    setIsBusy(true);
    setFeedback(null);

    try {
      const payload = {
        title: form.title,
        excerpt: form.excerpt || form.content.slice(0, 180),
        content: form.content,
        coverImageUrl: form.coverImageUrl || null,
        imageUrls: normalizeUrls(""),
        videoUrl: form.videoUrl || null,
        status: form.status,
        categoryIds: selectedCategories,
        tagIds: selectedTags,
        emissionIds: [],
        authorIds: [],
      };

      const url = mode === "edit" && articleId ? `${apiUrl}/api/Articles/${articleId}` : `${apiUrl}/api/Articles`;
      const method = mode === "edit" ? "PUT" : "POST";

      const response = await fetch(url, {
        method,
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token.trim()}`,
        },
        body: JSON.stringify(payload),
      });

      const responseBody = await response.json().catch(() => null);
      if (!response.ok) {
        const message =
          responseBody && typeof responseBody === "object" && "message" in responseBody
            ? String((responseBody as { message?: string }).message)
            : "Erreur lors de l'enregistrement.";
        throw new Error(message);
      }

      setFeedback(mode === "edit" ? "Article mis à jour." : "Article créé avec succès.");
      if (mode === "create") {
        setForm({
          title: "",
          excerpt: "",
          content: "# Titre\n\n## Sous-titre\n\nVotre article commence ici.",
          coverImageUrl: "",
          videoUrl: "",
          status: "Draft",
        });
        setSelectedCategories([]);
        setSelectedTags([]);
      } else {
        window.location.href = "/admin/articles";
      }
    } catch (error) {
      setFeedback(error instanceof Error ? error.message : "Erreur lors de l'enregistrement.");
    } finally {
      setIsBusy(false);
    }
  };

  return (
    <form className="stack" style={{ gap: 18 }} onSubmit={handleSubmit}>
      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">TOKEN JWT</label>
        <input
          className="editor-input"
          value={token}
          onChange={(event) => setToken(event.target.value)}
          placeholder="Collez le token JWT de l'admin ou rédacteur"
          style={{ minHeight: 48 }}
        />
        <button type="button" className="pixel-button" onClick={handleTokenSave}>Enregistrer le jeton</button>
      </div>

      <div className="split-grid" style={{ gridTemplateColumns: "1fr 1fr" }}>
        <div className="stack" style={{ gap: 10 }}>
          <label className="mono-font editor-label">TITRE</label>
          <input
            className="editor-input"
            value={form.title}
            onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
            placeholder="Titre de l'article"
            style={{ minHeight: 48 }}
          />
        </div>
        <div className="stack" style={{ gap: 10 }}>
          <label className="mono-font editor-label">STATUT</label>
          <select
            className="editor-input"
            value={form.status}
            onChange={(event) => setForm((current) => ({ ...current, status: event.target.value }))}
            style={{ minHeight: 48 }}
          >
            <option value="Draft">Brouillon</option>
            <option value="Published">Publié</option>
          </select>
        </div>
      </div>

      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">EXTRAIT</label>
        <textarea
          className="editor-input"
          value={form.excerpt}
          onChange={(event) => setForm((current) => ({ ...current, excerpt: event.target.value }))}
          placeholder="Résumé court de l'article"
          style={{ minHeight: 100 }}
        />
      </div>

      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">IMAGE DE COUVERTURE</label>
        <input
          className="editor-input"
          value={form.coverImageUrl}
          onChange={(event) => setForm((current) => ({ ...current, coverImageUrl: event.target.value }))}
          placeholder="https://..."
          style={{ minHeight: 48 }}
        />
      </div>

      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">VIDÉO</label>
        <input
          className="editor-input"
          value={form.videoUrl}
          onChange={(event) => setForm((current) => ({ ...current, videoUrl: event.target.value }))}
          placeholder="https://www.youtube.com/watch?v=..."
          style={{ minHeight: 48 }}
        />
      </div>

      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">CATÉGORIES</label>
        <div className="filter-group filter-group-tags" style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
          {categories.map((category) => (
            <button
              key={category.id}
              type="button"
              className={`filter-chip ${selectedCategories.includes(category.id) ? "active" : ""}`}
              onClick={() =>
                setSelectedCategories((current) =>
                  current.includes(category.id) ? current.filter((id) => id !== category.id) : [...current, category.id],
                )
              }
            >
              {category.name}
            </button>
          ))}
          {categories.length === 0 && <p className="article-excerpt">Aucune catégorie disponible.</p>}
        </div>
      </div>

      <div className="stack" style={{ gap: 10 }}>
        <label className="mono-font editor-label">TAGS</label>
        <div className="filter-group filter-group-tags" style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
          {tags.map((tag) => (
            <button
              key={tag.id}
              type="button"
              className={`filter-chip ${selectedTags.includes(tag.id) ? "active" : ""}`}
              onClick={() =>
                setSelectedTags((current) =>
                  current.includes(tag.id) ? current.filter((id) => id !== tag.id) : [...current, tag.id],
                )
              }
            >
              #{tag.name}
            </button>
          ))}
          {tags.length === 0 && <p className="article-excerpt">Aucun tag disponible.</p>}
        </div>
      </div>

      <MarkdownEditor value={form.content} onChange={(value) => setForm((current) => ({ ...current, content: value }))} />

      {feedback && (
        <div
          className="article-excerpt"
          style={{
            display: "flex",
            alignItems: "center",
            gap: 8,
            color: feedback.includes("succès") || feedback.includes("mis à jour") ? "#8ef0b7" : "#ffe8a3",
          }}
        >
          {feedback.includes("succès") || feedback.includes("mis à jour") ? <CheckCircle2 size={16} /> : <Save size={16} />}
          <span>{feedback}</span>
        </div>
      )}

      <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
        <button type="submit" className="pixel-button" disabled={!canSubmit || isBusy} style={{ width: "max-content" }}>
          {isBusy ? "Enregistrement..." : mode === "edit" ? "Mettre à jour" : "Publier le brouillon"}
        </button>
        <Link className="text-link" href="/admin/articles"><ArrowLeft size={16} />Retour à la liste</Link>
      </div>
    </form>
  );
}
