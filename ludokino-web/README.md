# Frontend Ludokino

Frontend public du site Ludokino, construit avec Next.js, TypeScript et Tailwind CSS.

Cette application reprend l'identite visuelle du site historique : fenetres Y2K, palette bleu nuit, logo LDKN, effet ecran cathodique et typographies d'origine.

## Installation

Depuis la racine du depot :

```powershell
npm --prefix ludokino-web install
```

## Developpement

```powershell
npm --prefix ludokino-web run dev
```

Le site est disponible sur http://localhost:3000.

## Validation

```powershell
npm --prefix ludokino-web run lint
npm --prefix ludokino-web run build
```

## Organisation

- `src/app/page.tsx` : page d'accueil et composants visuels de la fondation.
- `src/app/globals.css` : palette, fenetres Y2K, footer, responsive et effet CRT.
- `public/img/` : logo, favicon et assets provenant de l'ancien site.

## Identite visuelle

- `Libre Franklin` : navigation, titres et interfaces.
- `Space Grotesk` : texte courant.
- `JetBrains Mono` : informations systeme et metriques.
- `LDKN.svg` : logo officiel Ludokino, affiche en blanc dans la navigation.

## API

L'API locale est lancee separement :

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

La configuration CORS de l'API autorise le frontend local `http://localhost:3000` en developpement.
