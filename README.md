# Ludokino

Dépôt monorepo contenant une API ASP.NET Core, un frontend Next.js et les tests associés.

## Projets

```text
Ludokino.Api/          API REST .NET 8, EF Core, PostgreSQL et JWT
Ludokino.Api.Tests/    Tests unitaires, contrôleurs et intégration PostgreSQL
ludokino-web/          Frontend Next.js App Router
.github/workflows/     Workflow GitHub Actions de build et de tests
Ludokino.sln           Solution .NET de l'API et des tests
```

## Prérequis

- .NET SDK 8
- Node.js et npm
- PostgreSQL 14 ou supérieur
- Git

## Installation

```powershell
git clone https://github.com/Othakar/Ludokino.git
Set-Location Ludokino
dotnet restore Ludokino.sln
npm --prefix ludokino-web install
```

Créer `Ludokino.Api/appsettings.Development.json` localement :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ludokino;Username=postgres;Password=<mot-de-passe>"
  },
  "JwtSettings": {
    "SecretKey": "<cle-secrete-locale>",
    "Issuer": "LudokinoApi",
    "Audience": "LudokinoClient"
  }
}
```

Ce fichier est ignoré par Git.

## Démarrage local

Terminal 1, API :

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

Terminal 2, frontend :

```powershell
npm --prefix ludokino-web run dev
```

URLs locales :

- Frontend : http://localhost:3000
- API : selon l'URL affichée par ASP.NET Core
- Swagger : `<url-api>/swagger` en environnement Development

L'API applique les migrations EF Core au démarrage avant le seed initial. Les galeries d'articles sont stockées en PostgreSQL `jsonb` et les migrations convertissent les anciennes valeurs texte sans perte.

Le frontend expose les routes `/blog` et `/blog/{slug}`. Les articles peuvent contenir une couverture, une galerie d'images, une vidéo YouTube et un contenu Markdown rendu côté frontend.

## Configuration de production

Utiliser uniquement des variables d'environnement pour les secrets et les connexions :

```text
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
Cors__AllowedOrigins__0
API_URL
Youtube__ApiKey
Youtube__SyncIntervalMinutes
```

La synchronisation YouTube est exécutée côté API uniquement. La clé API ne doit jamais être exposée au frontend.

Les images externes des articles sont servies via la route frontend `/api/image`, limitée aux domaines d'images autorisés.

## Vérification locale

```powershell
dotnet build Ludokino.sln --configuration Release
dotnet test Ludokino.sln --configuration Release
npm --prefix ludokino-web run lint
npm --prefix ludokino-web run build
```

## Workflow Git

`main` est protégée. Chaque modification doit être réalisée sur une branche dédiée puis proposée par Pull Request :

```powershell
git switch main
git pull
git switch -c feature/nom-de-la-fonctionnalite
```

Après validation locale :

```powershell
git add .
git commit -m "Description technique de la modification"
git push -u origin feature/nom-de-la-fonctionnalite
```

La CI `build-and-test` doit réussir avant la fusion. Les branches fusionnées sont supprimées automatiquement.
