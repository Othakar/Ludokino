import Image from "next/image";
import Link from "next/link";
import {
  ArrowRight,
  CirclePlay,
  Download,
  Home as HomeIcon,
  Info,
  Monitor,
  Newspaper,
  Tv,
  Video,
  Zap,
  type LucideIcon,
} from "lucide-react";
import { siBluesky, siInstagram, siTiktok, siTwitch, siX, siYoutube } from "simple-icons";

const shows = [
  ["Monthly Wave", "L'émission musicale mensuelle qui explore les pépites sonores et les classiques oubliés."],
  ["TOKUKINO", "Héros en spandex moulant, monstres géants et explosions : notre format tokusatsu."],
  ["arka-TECH", "La high-tech de la fin des années 90 et du début des années 2000."],
  ["UNE DE MES JAPANIMATIONS", "Les œuvres d'animation japonaise, les bonnes comme les mauvaises."],
  ["TOONFLASH", "Une pastille qui recommande les œuvres animées du moment."],
  ["OMNIBUS", "Le magazine mensuel qui mélange toutes les émissions et quelques exclusivités."],
];

const articles = [
  ["9 AOÛT 2026 À 16H00", "3 FILMS TOKU POUR CET ÉTÉ", "Ultraman The Next, Gamera Vs Guiron et The Calamari Wrestler."],
  ["8 AOÛT 2026 À 18H30", "YOSHIKI ET KYARY PAMYU PAMYU", "LUDOKINO au plus près des grands artistes japonais à Japan Expo 2026."],
];

type BlueskyFeedItem = {
  reason?: unknown;
  post?: {
    author?: { handle?: string };
    uri?: string;
    record?: { text?: string };
  };
};

type LatestPost = {
  text: string;
  url: string;
};

async function getLatestBlueskyPost(): Promise<LatestPost> {
  const fallback = {
    text: "Suivez les dernières actualités de LUDOKINO sur Bluesky",
    url: "https://bsky.app/profile/ludokino.net",
  };

  try {
    const response = await fetch(
      "https://public.api.bsky.app/xrpc/app.bsky.feed.getAuthorFeed?actor=ludokino.net&limit=20",
      { next: { revalidate: 300 } },
    );

    if (!response.ok) return fallback;

    const data = (await response.json()) as { feed?: BlueskyFeedItem[] };
    const post = data.feed?.find(
      (item) => item.post?.author?.handle === "ludokino.net" && !item.reason,
    )?.post;
    const text = post?.record?.text?.trim();
    const rkey = post?.uri?.split("/").pop();

    if (!text || !rkey) return fallback;

    return {
      text,
      url: `https://bsky.app/profile/ludokino.net/post/${rkey}`,
    };
  } catch {
    return fallback;
  }
}

type BrandIconData = { path: string };

function BrandIcon({ icon, size = 22 }: { icon: BrandIconData; size?: number }) {
  return (
    <svg className="brand-icon" width={size} height={size} viewBox="0 0 24 24" aria-hidden="true">
      <path d={icon.path} />
    </svg>
  );
}

export function isSafeYoutubeUrl(value: unknown): value is string {
  if (typeof value !== "string") return false;

  try {
    const url = new URL(value);
    return url.protocol === "https:" && ["youtube.com", "www.youtube.com", "youtu.be"].includes(url.hostname);
  } catch {
    return false;
  }
}

function Window({ title, children, accent = false, Icon = Monitor }: { title: string; children: React.ReactNode; accent?: boolean; Icon?: LucideIcon }) {
  return (
    <section className={`window ${accent ? "info-window" : ""}`}>
      <div className="window-header">
        <span className="window-title pixel-font"><Icon size={19} strokeWidth={2.2} aria-hidden="true" />{title}</span>
        <span className="window-controls">
          <span className="window-control close" aria-hidden="true">×</span>
        </span>
      </div>
      <div className="window-body">{children}</div>
    </section>
  );
}

export function Navigation({ activeHref = "/" }: { activeHref?: string }) {
  const links = [
    ["Accueil", "/", HomeIcon],
    ["Emissions", "/shows", Tv],
    ["Blog", "/#articles", Newspaper],
    ["Goodies", "/#goodies", Download],
    ["À propos", "/#about", Info],
  ] as const;

  return (
    <nav className="site-nav" aria-label="Navigation principale">
      <Link className="brand" href="/" aria-label="Ludokino, accueil">
        <Image src="/img/LDKN.svg" alt="LUDOKINO" width={100} height={34} priority />
      </Link>
      <div className="nav-links">
        {links.map(([label, href, Icon]) => (
          <Link className={`nav-link pixel-font ${href === activeHref ? "active" : ""}`} href={href} key={label}>
            <Icon size={20} aria-hidden="true" />
            <span>{label}</span>
          </Link>
        ))}
      </div>
      <div className="social-links" aria-label="Réseaux sociaux">
        <a href="https://bsky.app/profile/ludokino.net" aria-label="Bluesky" title="Bluesky"><BrandIcon icon={siBluesky} /></a>
        <a href="https://www.twitch.tv/ludokino" aria-label="Twitch" title="Twitch"><BrandIcon icon={siTwitch} /></a>
        <a href="https://www.youtube.com/@ldkino" aria-label="YouTube" title="YouTube"><BrandIcon icon={siYoutube} /></a>
        <a href="https://www.tiktok.com/@ludokino" aria-label="TikTok" title="TikTok"><BrandIcon icon={siTiktok} /></a>
        <a href="https://x.com/ludokino" aria-label="X" title="X"><BrandIcon icon={siX} /></a>
      </div>
    </nav>
  );
}

export function Footer() {
  return (
    <footer className="site-footer">
      <div className="system-metrics mono-font">
        <span>CPU: 98%</span>
        <span>RAM: 512MB</span>
        <span>OS: LUDOKINO_OS v1.0</span>
      </div>
      <p className="footer-legal pixel-font">
        <span>© 2026 LUDOKINO - TOUS DROITS RÉSERVÉS</span>
        <span className="footer-divider">|</span>
        <Link href="/#mentions-legales">Mentions légales</Link>
      </p>
    </footer>
  );
}

export default async function Home() {
  const latestPost = await getLatestBlueskyPost();

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation />
      <div className="news-ticker" aria-label="Actualités Ludokino">
        <span className="ticker-badge pixel-font"><BrandIcon icon={siBluesky} size={16} />BSKY</span>
        <div className="ticker-track mono-font">
          <a className="ticker-copy" href={latestPost.url} target="_blank" rel="noreferrer">{latestPost.text}&nbsp; • &nbsp;</a>
          <span className="ticker-gap" aria-hidden="true" />
          <a className="ticker-copy" href={latestPost.url} target="_blank" rel="noreferrer" aria-hidden="true">{latestPost.text}&nbsp; • &nbsp;</a>
        </div>
      </div>
      <main className="page-shell">
        <div className="home-grid">
          <div className="stack">
            <Window title="LES OMNIBUS" Icon={Video}>
              <iframe className="video-frame" src="https://www.youtube-nocookie.com/embed/videoseries?list=PL13-SWMvlfiwijmK3dQ_rHY67bJj6rJ3P" title="Playlist Omnibus Ludokino" allowFullScreen />
            </Window>

            <div className="split-grid">
              <Window title="DERNIERS ARTICLES" Icon={Newspaper}>
                {articles.map(([date, title, excerpt]) => (
                  <article className="article-item" key={title}>
                    <p className="eyebrow mono-font">{date} — NEWS</p>
                    <h2 className="article-title pixel-font">{title}</h2>
                    <p className="article-excerpt">{excerpt}</p>
                    <a className="text-link" href="#articles">Lire l&apos;article <ArrowRight size={15} /></a>
                  </article>
                ))}
                <a className="pixel-button" href="#articles">Tous les articles</a>
              </Window>

              <Window title="LIENS RAPIDES" Icon={Zap}>
                <div className="quick-links">
                  <a className="quick-link" href="https://www.youtube.com/@ldkino"><BrandIcon icon={siYoutube} size={29} /><span>YouTube</span></a>
                  <a className="quick-link" href="https://www.twitch.tv/ludokino"><BrandIcon icon={siTwitch} size={29} /><span>Twitch</span></a>
                  <a className="quick-link" href="https://www.instagram.com/ludokino_/"><BrandIcon icon={siInstagram} size={29} /><span>Instagram</span></a>
                  <a className="quick-link" href="https://bsky.app/profile/ludokino.net"><BrandIcon icon={siBluesky} size={29} /><span>Bluesky</span></a>
                  <a className="quick-link" href="https://www.tiktok.com/@ludokino"><BrandIcon icon={siTiktok} size={29} /><span>TikTok</span></a>
                  <a className="quick-link" href="https://x.com/ludokino"><BrandIcon icon={siX} size={29} /><span>X</span></a>
                </div>
              </Window>
            </div>
          </div>

          <aside className="stack">
            <Window title="INFO.SYS" accent Icon={Monitor}>
              <div className="info-list mono-font">
                <div className="info-row"><span className="info-label">STATUS:</span><span className="status-offline">OFFLINE</span></div>
                <div className="info-row"><span className="info-label">VERSION:</span><span>2026.08</span></div>
                <div className="info-row"><span className="info-label">LOCATION:</span><span>FRANCE / WEB</span></div>
                <p>“LUDOKINO est votre média culture, jeu vidéo geek et otaku. LDKN pour les intimes.”</p>
              </div>
            </Window>

            <Window title="NOS ÉMISSIONS" Icon={CirclePlay}>
              <div className="show-list">
                {shows.map(([title, description]) => (
                  <a className="show-item" href="#emissions" key={title}>
                    <h2 className="show-name">{title}</h2>
                    <p className="show-description">{description}</p>
                  </a>
                ))}
              </div>
            </Window>

            <Window title="ESPACE PUBLICITAIRE" Icon={Monitor}>
              <div className="ad-space pixel-font">On n&apos;a pas encore trouvé de sponsor,<br />mais ça pourrait être vous !</div>
            </Window>
          </aside>
        </div>
      </main>
      <Footer />
    </div>
  );
}
