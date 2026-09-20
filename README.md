# LUDOKINO

Ludokino est un média indépendant consacré aux jeux vidéo, à l'animation japonaise, au tokusatsu, à la musique et à la culture geek.

Le dépôt contient l'API métier, les tests automatisés et la première fondation du frontend reprenant l'identité visuelle historique du site : fenêtres Y2K, palette bleu nuit, logo LDKN et typographies du site original.

## Structure du dépôt

```text
Ludokino.Api/          API ASP.NET Core 8, PostgreSQL et migrations EF Core
Ludokino.Api.Tests/    Tests unitaires, contrôleurs et intégration PostgreSQL
ludokino-web/          Frontend Next.js et interface publique Ludokino
.github/workflows/     CI GitHub Actions
Ludokino.sln           Solution .NET de l'API et des tests
```

## Prérequis

- .NET SDK 8
- Node.js et npm
- PostgreSQL 14 ou supérieur pour l'API et les tests d'intégration
- Git

## Installation

```powershell
git clone https://github.com/Othakar/Ludokino.git
Set-Location Ludokino
dotnet restore Ludokino.sln
Set-Location ludokino-web
npm install
Set-Location ..
```

Créer localement `Ludokino.Api/appsettings.Development.json`. Ce fichier est ignoré par Git :

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

## Lancer le projet

Dans un premier terminal, lancer l'API :

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

Dans un second terminal, lancer le frontend :

```powershell
npm --prefix ludokino-web run dev
```

Le frontend est disponible sur http://localhost:3000. Swagger est disponible sur `/swagger` lorsque l'API tourne en environnement de développement.

Les migrations EF Core sont appliquées automatiquement au démarrage de l'API avant le seed initial. Pour les appliquer manuellement :

```powershell
dotnet ef database update --project Ludokino.Api/Ludokino.Api.csproj
```

## Tests

Lancer la suite .NET :

```powershell
dotnet test Ludokino.sln --configuration Release
```

Les tests PostgreSQL utilisent exclusivement une base dédiée `ludokino_test` :

```powershell
$env:LUDOKINO_TEST_CONNECTION_STRING = "Host=localhost;Database=ludokino_test;Username=postgres;Password=<mot-de-passe>"
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj --configuration Release
Remove-Item Env:LUDOKINO_TEST_CONNECTION_STRING
```

Sans cette variable, le test PostgreSQL est ignoré. Dans GitHub Actions, PostgreSQL est démarré automatiquement dans un service éphémère et ce test est exécuté.

## API et sécurité

- Authentification JWT pour les rôles `Admin` et `Redacteur`.
- Articles publiés accessibles publiquement; brouillons réservés aux comptes autorisés.
- Catégories, tags et émissions gérés par des services et contrôleurs dédiés.
- Une émission exige un lien YouTube HTTPS valide.
- Migrations EF Core PostgreSQL versionnées dans `Ludokino.Api/Migrations`.
- Headers HTTP de sécurité activés dans l'API.
- CORS limité aux origines configurées; en développement, `http://localhost:3000` est autorisé.

En production, fournir les paramètres par variables d'environnement :

```text
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
Cors__AllowedOrigins__0
```

Ne jamais versionner de mot de passe, token ou chaîne de connexion réelle.

## Workflow Git

La branche `main` est protégée. Chaque fonctionnalité doit utiliser sa propre branche :

```powershell
git switch main
git pull
git switch -c feature/ma-fonctionnalite
```

Avant de créer une Pull Request :

```powershell
dotnet build Ludokino.sln --configuration Release
dotnet test Ludokino.sln --configuration Release
npm --prefix ludokino-web run lint
npm --prefix ludokino-web run build
git add .
git commit -m "Décrit la modification"
git push -u origin feature/ma-fonctionnalite
```

La Pull Request vers `main` doit passer la vérification `build-and-test`. Les branches sont supprimées automatiquement après fusion.
