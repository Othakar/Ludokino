import { ExternalLink, Monitor, PlayCircle } from "lucide-react";
import { Footer, isSafeYoutubeUrl, Navigation } from "../page";

const fallbackShows = [
  { id: 1, name: "Monthly Wave", slug: "monthly-wave", description: "L'émission musicale mensuelle qui explore les pépites sonores et les classiques oubliés.", type: "Musique", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfix9XYhGjoKQdoL0Je81qD3l", imageUrl: null },
  { id: 2, name: "TOKUKINO", slug: "tokukino", description: "Héros en spandex moulant, monstres géants et explosions : le format consacré au tokusatsu.", type: "Tokusatsu", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfix9cWf78JWzGAdsksUj4MuR", imageUrl: null },
  { id: 3, name: "arka-TECH", slug: "arka-tech", description: "Redécouvre la high-tech de la fin des années 90 et du début des années 2000.", type: "Tech", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfiz2XSS9Z2N6RNfVpluRk_yT", imageUrl: null },
  { id: 4, name: "UNE DE MES JAPANIMATIONS", slug: "une-de-mes-japanimations", description: "Kagano met en avant des œuvres d'animation japonaise, les bonnes comme les mauvaises.", type: "Animation", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfixjwSVuDGfQplh3jBj46Jg1", imageUrl: null },
  { id: 5, name: "TOONFLASH", slug: "toonflash", description: "Une pastille qui recommande les œuvres animées du moment.", type: "Animation", youtubeUrl: "https://www.youtube.com/playlist?list=PLYP9p4UelR2QqwIDScjeXGzxSQAuxtOUC", imageUrl: null },
  { id: 6, name: "OMNIBUS", slug: "omnibus", description: "Le magazine mensuel de LUDOKINO, avec toutes les émissions et quelques exclusivités.", type: "Magazine", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfiwijmK3dQ_rHY67bJj6rJ3P", imageUrl: null },
  { id: 7, name: "In Paris", slug: "in-paris", description: "Wendöh et VincenTimes cherchent le meilleur sandwich merguez dans Paris et parlent de tout et de rien.", type: "Culture", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfix9dXj9wNYqGCyAQbdHmPJf", imageUrl: null },
  { id: 8, name: "Critique Contemporaine", slug: "critique-contemporaine", description: "La critique du jeu qui vient de sortir. À chaud, sans filtre.", type: "Jeu vidéo", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfixBk5H2gNH1Q8QugDhyQ75_", imageUrl: null },
  { id: 9, name: "Critique Flashback", slug: "critique-flashback", description: "Retour sur les jeux sortis avant l'ère Xbox 360 et PlayStation 3.", type: "Jeu vidéo", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfixcLLe3_zcB7Ow1a9p9K4th", imageUrl: null },
  { id: 10, name: "Dossiers", slug: "dossiers", description: "Des vidéos sur des sujets spécifiques qui n'entrent dans aucune case.", type: "Dossier", youtubeUrl: "https://www.youtube.com/playlist?list=PL13-SWMvlfiwbUkhTXT7oMmNl8y5haJ6q", imageUrl: null },
];

type Emission = (typeof fallbackShows)[number];

function isEmission(value: unknown): value is Emission {
  if (!value || typeof value !== "object") return false;

  const emission = value as Partial<Emission>;
  return (
    typeof emission.id === "number" &&
    typeof emission.name === "string" &&
    typeof emission.slug === "string" &&
    typeof emission.description === "string" &&
    typeof emission.type === "string" &&
    isSafeYoutubeUrl(emission.youtubeUrl) &&
    (emission.imageUrl === null || typeof emission.imageUrl === "string")
  );
}

async function getEmissions(): Promise<Emission[]> {
  const apiUrl = process.env.API_URL ?? (process.env.NODE_ENV === "development" ? "http://localhost:5000" : undefined);
  const fallback = process.env.NODE_ENV === "development" ? fallbackShows : [];
  if (!apiUrl) return fallback;

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 2500);

  try {
    const response = await fetch(`${apiUrl}/api/Emissions`, {
      next: { revalidate: 60 },
      signal: controller.signal,
    });
    if (!response.ok) return fallback;

    const data: unknown = await response.json();
    if (!Array.isArray(data)) return fallback;

    const emissions = data.filter(isEmission);
    const uniqueEmissions = new Map<string, Emission>();
    for (const emission of emissions) {
      const key = emission.name.trim().toLocaleLowerCase();
      if (!uniqueEmissions.has(key)) uniqueEmissions.set(key, emission);
    }

    if (process.env.NODE_ENV === "development") {
      for (const fallback of fallbackShows) {
        const key = fallback.name.trim().toLocaleLowerCase();
        if (!uniqueEmissions.has(key)) uniqueEmissions.set(key, fallback);
      }
    }

    return [...uniqueEmissions.values()];
  } catch {
    return fallback;
  } finally {
    clearTimeout(timeout);
  }
}

function Window({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="window">
      <div className="window-header">
        <span className="window-title pixel-font"><Monitor size={19} aria-hidden="true" />{title}</span>
        <span className="window-controls"><span className="window-control close" aria-hidden="true">×</span></span>
      </div>
      <div className="window-body">{children}</div>
    </section>
  );
}

export default async function ShowsPage() {
  const emissions = await getEmissions();

  return (
    <div>
      <div className="scanline" />
      <div className="crt-overlay" aria-hidden="true" />
      <Navigation activeHref="/shows" />
      <main className="page-shell shows-page">
        <div className="shows-intro">
          <p className="eyebrow mono-font">MEDIA_INDEX / VIDEO_ARCHIVE</p>
          <h1 className="shows-heading pixel-font">NOS ÉMISSIONS</h1>
          <p>De la musique à la tech, en passant par le tokusatsu, l&apos;animation et le jeu vidéo.</p>
        </div>
        <div className="shows-grid">
          {emissions.map((emission) => (
            <Window title={emission.name} key={emission.slug}>
              <article className="show-card">
                <div className={`show-thumb ${emission.imageUrl ? "has-image" : ""}`}>
                  {emission.imageUrl && (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={emission.imageUrl} alt={`Miniature de ${emission.name}`} />
                  )}
                  <span className="show-thumb-play">
                    <PlayCircle size={46} aria-hidden="true" />
                  </span>
                </div>
                <div className="show-card-content">
                  <span className="show-type mono-font">{emission.type || "ÉMISSION"}</span>
                  <h2 className="show-card-title">{emission.name}</h2>
                  <p>{emission.description}</p>
                  <a className="pixel-button" href={emission.youtubeUrl} target="_blank" rel="noreferrer">
                    Voir les épisodes <ExternalLink size={15} />
                  </a>
                </div>
              </article>
            </Window>
          ))}
        </div>
        {emissions.length === 0 && (
          <p className="shows-empty">Aucune émission n&apos;est disponible pour le moment.</p>
        )}
        <Window title="OMNIBUS : LA DÉFINITION">
          <div className="omnibus-definition">
            <h2 className="shows-heading pixel-font">QU&apos;EST-CE QU&apos;UN OMNIBUS ?</h2>
            <p>C&apos;est le format ultime de LUDOKINO. Chaque mois, nous mélangeons toutes nos émissions, nos sketchs et nos intermittences funs pour créer un montage final massif pouvant durer jusqu&apos;à <strong>2 HEURES*</strong>.</p>
            <small>*Pas tout le temps non plus : parfois 2h30, parfois 1h45...</small>
            <div className="omnibus-specs mono-font"><span>50 FPS</span><span>1080P</span><span>STEREO</span></div>
          </div>
        </Window>
      </main>
      <Footer />
    </div>
  );
}
