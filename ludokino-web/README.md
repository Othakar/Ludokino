# ludokino-web

Frontend Next.js 16 avec TypeScript et App Router.

## Installation

Depuis la racine du dépôt :

```powershell
npm --prefix ludokino-web install
```

## Variables d'environnement

Créer éventuellement `ludokino-web/.env.local` :

```text
API_URL=http://localhost:5000
```

En production, `API_URL` doit pointer vers l'API publique. Si elle est absente en développement, la page émissions utilise son catalogue local de secours. En production, l'absence de l'API produit un état vide plutôt que des données de développement.

## Commandes

```powershell
npm --prefix ludokino-web run dev
npm --prefix ludokino-web run lint
npm --prefix ludokino-web run smoke
npm --prefix ludokino-web run build
npm --prefix ludokino-web run start
```

Le serveur de développement écoute par défaut sur http://localhost:3000.

## Fonctionnement

- `src/app/page.tsx` : page d'accueil, navigation partagée et footer partagé.
- `src/app/shows/page.tsx` : page des émissions, chargée depuis `/api/Emissions`.
- `src/app/blog/page.tsx` : liste des articles, filtres catégories/tags et sélection compacte des tags.
- `src/app/blog/[slug]/page.tsx` : détail d'article, contenu Markdown, vidéos et galeries d'images.
- `src/app/api/image/route.ts` : proxy sécurisé pour les images externes autorisées des articles.
- `src/app/globals.css` : styles globaux, responsive, headers de fenêtres et protections visuelles.
- `proxy.ts` : filtrage des chemins suspects et des ressources publiques non autorisées.
- `next.config.ts` : headers HTTP de sécurité et configuration Next.js.
- `public/img/` : ressources statiques explicitement utilisées par le frontend.

## Appel API émissions

La page `/shows` appelle :

```text
GET ${API_URL}/api/Emissions
```

Elle vérifie la forme minimale de chaque élément, impose une URL YouTube HTTPS pour les liens de lecture et utilise un fallback local uniquement en développement lorsque l'API est indisponible.

## Sécurité frontend

Le frontend configure notamment :

- CSP;
- HSTS en production;
- `X-Content-Type-Options`;
- `X-Frame-Options`;
- `Referrer-Policy`;
- `Permissions-Policy`;
- filtrage des chemins de traversal et des dotfiles.

Les ressources placées dans `public/` sont publiques par définition. Ne jamais y placer de secret, token ou fichier de configuration.

## Blog

La page `/blog` consomme `GET ${API_URL}/api/Articles?page=1&pageSize=30`. Les catégories et tags sont filtrables séparément ; douze tags sont affichés dans la vue compacte et le bouton `Tous les tags` ouvre ou réduit la liste complète.

La page détail rend le contenu Markdown avec `react-markdown`. Les URLs d'images provenant de l'API sont chargées via `/api/image`, qui n'accepte que les hôtes d'images configurés dans la route.

Le smoke test `npm run smoke` vérifie les endpoints API des émissions et articles, les routes `/shows` et `/blog/{slug}`, ainsi que le proxy des miniatures. Les URLs peuvent être remplacées avec `API_URL` et `WEB_URL`.
