# Ludokino
API backend du site média Ludokino, construite avec ASP.NET Core 8, Entity Framework Core et PostgreSQL.

## Contenu du dépôt

- `Ludokino.Api` : API REST, authentification JWT, services métier, contrôleurs et migrations EF Core.
- `Ludokino.Api.Tests` : tests unitaires, tests de contrôleurs et test d'intégration PostgreSQL.
- `Ludokino.sln` : solution .NET regroupant l'API et les tests.

## Prérequis

- .NET SDK 8
- PostgreSQL 14 ou supérieur pour le développement et les tests d'intégration
- Git

## Installation

Cloner le dépôt puis restaurer les dépendances :

```powershell
git clone https://github.com/Othakar/Ludokino.git
Set-Location Ludokino
dotnet restore Ludokino.sln
```

Créer `Ludokino.Api/appsettings.Development.json` localement. Ce fichier est ignoré par Git :

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Host=localhost;Database=ludokino;Username=postgres;Password=<mot-de-passe>"
	},
	"JwtSettings": {
		"SecretKey": "<cle-secrete-locale-de-developpement>",
		"Issuer": "LudokinoApi",
		"Audience": "LudokinoClient"
	}
}
```

La base `ludokino` est créée et migrée automatiquement au démarrage de l'API. Les migrations peuvent aussi être appliquées manuellement :

```powershell
dotnet ef database update --project Ludokino.Api/Ludokino.Api.csproj
```

## Lancer l'API

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

Swagger est disponible en environnement de développement sur `/swagger`.

## Tester

Lancer les tests unitaires et de contrôleurs :

```powershell
dotnet test Ludokino.sln
```

Le test PostgreSQL utilise une base séparée nommée `ludokino_test`. Il ne doit jamais utiliser la base de développement ou la base de production :

```powershell
$env:LUDOKINO_TEST_CONNECTION_STRING = "Host=localhost;Database=ludokino_test;Username=postgres;Password=<mot-de-passe>"
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj
Remove-Item Env:LUDOKINO_TEST_CONNECTION_STRING
```

Sans cette variable, le test PostgreSQL est ignoré et les tests sans dépendance externe continuent de s'exécuter.

## Fonctionnalités actuelles

- Authentification JWT pour les rôles `Admin` et `Redacteur`.
- Articles avec brouillons, publication, catégories, tags, émissions et auteurs multiples.
- Émissions vidéo avec lien YouTube HTTPS obligatoire.
- Endpoints publics pour les articles publiés, catégories, tags, émissions et équipe.
- Endpoints protégés pour la gestion éditoriale et l'administration.
- Migrations EF Core PostgreSQL appliquées au démarrage avant le seed.
- Headers HTTP de sécurité et CI GitHub Actions sur les Pull Requests.

## Workflow Git

La branche `main` est protégée. Pour contribuer :

```powershell
git switch main
git pull
git switch -c feature/ma-fonctionnalite
```

Après les modifications :

```powershell
dotnet build Ludokino.sln --configuration Release
dotnet test Ludokino.sln --configuration Release
git add .
git commit -m "Décrit la modification"
git push -u origin feature/ma-fonctionnalite
```

Ouvrir ensuite une Pull Request vers `main`. La CI doit réussir avant la fusion.

## Configuration de production

Ne jamais committer de mot de passe, token ou chaîne de connexion. Fournir la base PostgreSQL et les paramètres JWT via variables d'environnement, notamment :

```text
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
```
