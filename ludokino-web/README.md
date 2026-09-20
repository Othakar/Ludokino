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
npm --prefix ludokino-web run build
npm --prefix ludokino-web run start
```

Le serveur de développement écoute par défaut sur http://localhost:3000.

## Fonctionnement

- `src/app/page.tsx` : page d'accueil, navigation partagée et footer partagé.
- `src/app/shows/page.tsx` : page des émissions, chargée depuis `/api/Emissions`.
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
