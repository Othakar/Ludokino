import Link from "next/link";
import { ArrowLeft, Monitor } from "lucide-react";
import { Footer, Navigation, Window } from "../../page";

type Article = {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl?: string | null;
  publishedAt?: string | null;
  tags: string[];
  categories: { name: string; slug: string }[];
};

async function getArticle(slug: string): Promise<Article | null> {
  const apiUrl = process.env.API_URL ?? (process.env.NODE_ENV === "development" ? "http://localhost:5000" : undefined);
  if (!apiUrl) return null;

  try {
    const response = await fetch(`${apiUrl}/api/Articles/${encodeURIComponent(slug)}`, { next: { revalidate: 60 } });
    return response.ok ? ((await response.json()) as Article) : null;
  } catch {
    return null;
  }
}

export default async function ArticlePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const article = await getArticle(slug);

  if (!article) {
    return (
      <div>
        <Navigation />
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
      <main className="page-shell blog-detail-page">
        <Link className="text-link" href="/blog"><ArrowLeft size={16} />Retour aux articles</Link>
        <Window title={article.categories[0]?.name || "ARTICLE"} Icon={Monitor}>
          <article className="article-detail">
            <p className="eyebrow mono-font">{article.publishedAt || ""}</p>
            <h1 className="shows-heading pixel-font">{article.title}</h1>
            <p className="article-detail-excerpt">{article.excerpt}</p>
            {article.coverImageUrl && (
              // eslint-disable-next-line @next/next/no-img-element
              <img className="article-cover" src={article.coverImageUrl} alt={`Illustration de ${article.title}`} />
            )}
            <div className="article-content">{article.content}</div>
          </article>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
