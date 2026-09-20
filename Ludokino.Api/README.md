# API Ludokino

API REST du site Ludokino, construite avec ASP.NET Core 8, Entity Framework Core et PostgreSQL.

## Configuration locale

Créer `appsettings.Development.json` à côté du fichier projet :

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

En développement, le frontend `http://localhost:3000` est autorisé par défaut via CORS. En production, définir explicitement les origines :

```text
Cors__AllowedOrigins__0=https://www.exemple.fr
Cors__AllowedOrigins__1=https://admin.exemple.fr
```

La production doit fournir la chaîne PostgreSQL uniquement via `ConnectionStrings__DefaultConnection`.

## Base de données

L'API applique automatiquement les migrations EF Core au démarrage, puis exécute le seed initial. La base `ludokino` doit être accessible avec l'utilisateur configuré.

Appliquer les migrations manuellement :

```powershell
dotnet ef database update --project Ludokino.Api.csproj
```

Créer une migration après une modification du modèle :

```powershell
dotnet ef migrations add NomDeLaMigration --project Ludokino.Api.csproj --output-dir Migrations
```

## Lancement

Depuis la racine du dépôt :

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

Swagger est disponible sur `/swagger` en environnement de développement.

## Fonctionnalités

- Authentification JWT pour `Admin` et `Redacteur`.
- Articles, brouillons, publication, auteurs multiples, catégories, tags et émissions.
- Émissions vidéo avec lien YouTube HTTPS obligatoire.
- Équipe publique et endpoints d'administration protégés par rôle.
- Headers HTTP de sécurité et politique CORS explicite.
