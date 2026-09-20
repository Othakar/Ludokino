import Link from "next/link";
import { Hash, Layers3, Tag } from "lucide-react";
import { Footer, Navigation, Window } from "../page";

type Article = {
  id: number;
  title: string;
  slug: string;
  excerpt: string;
  coverImageUrl?: string | null;
  publishedAt?: string | null;
  tags: string[];
  categories: string[];
};

const fallbackArticles: Article[] = [
  { id: 1, title: "3 FILMS TOKU POUR CET ÉTÉ", slug: "tkkn-3films-ete", excerpt: "Ultraman The Next, Gamera Vs Guiron et The Calamari Wrestler.", publishedAt: "2026-08-08T18:30:00Z", tags: ["Tokusatsu"], categories: ["News"] },
  { id: 2, title: "YOSHIKI ET KYARY PAMYU PAMYU", slug: "yoshiki-kyary-japanexpo25", excerpt: "LUDOKINO au plus près des artistes japonais à Japan Expo 2026.", publishedAt: "2026-08-08T16:00:00Z", tags: ["Japon"], categories: ["News"] },
];

function isArticle(value: unknown): value is Article {
  if (!value || typeof value !== "object") return false;
  const article = value as Partial<Article>;
  return typeof article.id === "number" && typeof article.title === "string" && typeof article.slug === "string" && typeof article.excerpt === "string" && Array.isArray(article.tags) && Array.isArray(article.categories);
}

async function getArticles(): Promise<Article[]> {
  const apiUrl = process.env.API_URL ?? (process.env.NODE_ENV === "development" ? "http://localhost:5000" : undefined);
  if (!apiUrl) return [];

  try {
    const response = await fetch(`${apiUrl}/api/Articles?page=1&pageSize=30`, { next: { revalidate: 60 } });
    if (!response.ok) return process.env.NODE_ENV === "development" ? fallbackArticles : [];
    const data: unknown = await response.json();
    return Array.isArray(data) ? data.filter(isArticle) : [];
  } catch {
    return process.env.NODE_ENV === "development" ? fallbackArticles : [];
  }
}

function formatDate(value?: string | null) {
  if (!value) return "Date inconnue";
  return new Intl.DateTimeFormat("fr-FR", { dateStyle: "medium" }).format(new Date(value));
}

function getImageUrl(value?: string | null) {
  return value ? `/api/image?url=${encodeURIComponent(value)}` : "";
}

function getRandomTags(tags: string[], selectedTag?: string) {
  const selectedTagValue = tags.find((tag) => tag.toLowerCase() === selectedTag);
  const remainingTags = tags.filter((tag) => tag !== selectedTagValue);

  for (let index = remainingTags.length - 1; index > 0; index--) {
    const randomIndex = Math.floor(Math.random() * (index + 1));
    [remainingTags[index], remainingTags[randomIndex]] = [remainingTags[randomIndex], remainingTags[index]];
  }

  const selectedTags = selectedTagValue
    ? [selectedTagValue, ...remainingTags.slice(0, 11)]
    : remainingTags.slice(0, 12);

  return selectedTags.sort((left, right) => left.localeCompare(right, "fr", { sensitivity: "base" }));
}

export default async function BlogPage({ searchParams }: { searchParams: Promise<{ tag?: string; category?: string; tags?: string }> }) {
  const [articles, params] = await Promise.all([getArticles(), searchParams]);
  const selectedTag = params.tag?.trim().toLowerCase();
  const selectedCategory = params.category?.trim().toLowerCase();
  const filteredArticles = articles.filter((article) => {
    const matchesTag = !selectedTag || article.tags.some((tag) => tag.toLowerCase() === selectedTag);
    const matchesCategory = !selectedCategory || article.categories.some((category) => category.toLowerCase() === selectedCategory);
    return matchesTag && matchesCategory;
  });
  const tags = [...new Set(articles.flatMap((article) => article.tags))].sort();
  const categories = [...new Set(articles.flatMap((article) => article.categories))].sort();
  const showAllTags = params.tags === "all";
  const visibleTags = showAllTags ? tags : getRandomTags(tags, selectedTag);
  const tagsQuery = selectedCategory ? `&category=${encodeURIComponent(params.category ?? "")}` : "";
  const allTagsHref = showAllTags
    ? (selectedCategory ? `/blog?category=${encodeURIComponent(params.category ?? "")}` : "/blog")
    : `/blog?tags=all${tagsQuery}`;

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/blog" />
      <main className="page-shell blog-page">
        <div className="shows-intro">
          <p className="eyebrow mono-font">PUBLICATIONS / ARCHIVE</p>
          <h1 className="shows-heading pixel-font">DERNIERS ARTICLES</h1>
          <p>Retrouvez les articles publiés et filtrez-les par thème.</p>
        </div>
        <div className="blog-filters">
          <div className="filter-group filter-group-categories">
            <div className="filter-group-heading"><Layers3 size={15} /> <span className="mono-font">CATÉGORIES</span></div>
            <div className="filter-options">
              <Link className={!selectedTag && !selectedCategory ? "filter-chip active" : "filter-chip"} href="/blog"><Layers3 size={15} />Toutes catégories</Link>
              {categories.map((category) => <Link className={selectedCategory === category.toLowerCase() ? "filter-chip active" : "filter-chip"} href={`/blog?category=${encodeURIComponent(category)}`} key={`category-${category}`}><Layers3 size={15} />{category}</Link>)}
            </div>
          </div>
          <div className="filter-group filter-group-tags">
            <div className="filter-group-heading"><Hash size={15} /> <span className="mono-font">TAGS</span></div>
            <div className="filter-options">
                <Link className={showAllTags ? "filter-chip active" : "filter-chip"} href={allTagsHref}><Tag size={14} />{showAllTags ? "Réduire les tags" : "Tous les tags"}</Link>
              {visibleTags.map((tag) => <Link className={selectedTag === tag.toLowerCase() ? "filter-chip active" : "filter-chip"} href={`/blog?tag=${encodeURIComponent(tag)}${tagsQuery}`} key={`tag-${tag}`}><Tag size={14} />{tag}</Link>)}
            </div>
          </div>
        </div>
        <div className="blog-grid">
          {filteredArticles.map((article) => (
            <Window title={article.categories[0] || article.tags[0] || "ARTICLE"} key={article.slug}>
              <article className="blog-card">
                {article.coverImageUrl && (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={getImageUrl(article.coverImageUrl)} alt={`Illustration de ${article.title}`} />
                )}
                <div className="blog-card-meta mono-font">{formatDate(article.publishedAt)}</div>
                <div className="article-taxonomy" aria-label="Catégorie et tags">
                  {article.categories.map((category) => <Link href={`/blog?category=${encodeURIComponent(category)}`} key={`category-${category}`}>{category}</Link>)}
                  {article.tags.map((tag) => <Link href={`/blog?tag=${encodeURIComponent(tag)}`} key={`tag-${tag}`}>#{tag}</Link>)}
                </div>
                <h2 className="blog-card-title">{article.title}</h2>
                <p>{article.excerpt}</p>
                <Link className="pixel-button" href={`/blog/${article.slug}`}>Lire l&apos;article</Link>
              </article>
            </Window>
          ))}
        </div>
        {filteredArticles.length === 0 && <p className="shows-empty">Aucun article ne correspond aux filtres.</p>}
      </main>
      <Footer />
    </div>
  );
}
