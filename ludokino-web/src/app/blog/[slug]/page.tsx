import Link from "next/link";
import { ArrowLeft, Monitor, Play } from "lucide-react";
import ReactMarkdown from "react-markdown";
import { Footer, Navigation, Window } from "../../page";

type Article = {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl?: string | null;
  imageUrls?: string[];
  videoUrl?: string | null;
  publishedAt?: string | null;
  tags: { name: string; slug: string }[];
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

function getYoutubeVideoId(value?: string | null) {
  return value?.match(/[?&]v=([^&]+)/)?.[1] ?? value?.match(/youtu\.be\/([^?]+)/)?.[1] ?? null;
}

function getImageUrl(value?: string | null) {
  return value ? `/api/image?url=${encodeURIComponent(value)}` : "";
}

export default async function ArticlePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const article = await getArticle(slug);
  const videoId = getYoutubeVideoId(article?.videoUrl);
  const imageUrls = article?.imageUrls ?? [];

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
        <Window title={article.categories[0]?.name || article.tags[0]?.name || "ARTICLE"} Icon={Monitor}>
          <article className="article-detail">
            <p className="eyebrow mono-font">{article.publishedAt || ""}</p>
            <h1 className="shows-heading pixel-font">{article.title}</h1>
            <div className="article-taxonomy" aria-label="Catégorie et tags">
              {article.categories.map((category) => <Link href={`/blog?category=${encodeURIComponent(category.slug)}`} key={category.slug}>{category.name}</Link>)}
              {article.tags.map((tag) => <Link href={`/blog?tag=${encodeURIComponent(tag.name)}`} key={tag.slug}>#{tag.name}</Link>)}
            </div>
            <p className="article-detail-excerpt">{article.excerpt}</p>
            {article.coverImageUrl && videoId ? (
              <div className="article-video">
                <a href={article.videoUrl ?? `https://www.youtube.com/watch?v=${videoId}`} target="_blank" rel="noreferrer" aria-label={`Regarder la vidéo de ${article.title}`}>
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img className="article-cover" src={getImageUrl(article.coverImageUrl)} alt={`Miniature de la vidéo de ${article.title}`} />
                  <span className="article-video-link"><Play size={16} fill="currentColor" />Regarder la vidéo</span>
                </a>
              </div>
            ) : article.coverImageUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img className="article-cover" src={getImageUrl(article.coverImageUrl)} alt={`Illustration de ${article.title}`} />
            ) : null}
            {imageUrls.length > 0 && (
              <div className="article-gallery" aria-label="Images de l'article">
                {imageUrls.map((imageUrl, index) => (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={getImageUrl(imageUrl)} alt={`${article.title} - image ${index + 1}`} key={imageUrl} />
                ))}
              </div>
            )}
            <div className="article-content">
              <ReactMarkdown components={{
                img: ({ src, alt }) => (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={getImageUrl(typeof src === "string" ? src : null)} alt={alt ?? ""} />
                ),
              }}>
                {article.content}
              </ReactMarkdown>
            </div>
          </article>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
