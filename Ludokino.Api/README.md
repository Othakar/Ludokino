# Ludokino.Api

API REST ASP.NET Core 8 avec Entity Framework Core, PostgreSQL, JWT et Swagger.

## Configuration locale

Créer `appsettings.Development.json` à côté de `Ludokino.Api.csproj` :

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

En développement, CORS autorise `http://localhost:3000`. En production, déclarer les origines avec :

```text
Cors__AllowedOrigins__0=https://exemple.fr
Cors__AllowedOrigins__1=https://admin.exemple.fr
```

## Synchronisation YouTube

La synchronisation est inactive tant que `Youtube__ApiKey` n'est pas définie. Pour l'activer :

```text
Youtube__ApiKey=<cle-api-youtube-data-v3>
Youtube__SyncIntervalMinutes=60
```

Le worker récupère les dernières vidéos des playlists enregistrées dans `Emission.PlaylistUrl` et met à jour `LatestVideoId`, `YoutubeUrl`, `ThumbnailUrl` et `LastSyncedAt`.

Une synchronisation ponctuelle peut être déclenchée par un administrateur :

```text
POST /api/Emissions/sync-youtube
```

La clé YouTube reste uniquement côté API et ne doit jamais être exposée au frontend.

## Base de données

Au démarrage, l'API :

1. applique les migrations présentes dans `Migrations/`;
2. crée le schéma si nécessaire;
3. exécute le seed initial des rôles et utilisateurs.

Commandes EF Core :

```powershell
dotnet ef database update --project Ludokino.Api.csproj
dotnet ef migrations list --project Ludokino.Api.csproj
dotnet ef migrations add NomDeMigration --project Ludokino.Api.csproj --output-dir Migrations
```

La base de test doit être séparée de `ludokino`.

## Démarrage

Depuis la racine :

```powershell
dotnet run --project Ludokino.Api/Ludokino.Api.csproj
```

Swagger est disponible sur `<url-api>/swagger` en environnement Development.

## Services et routes

- Authentification : `/api/Auth`
- Articles : `/api/Articles`
- Catégories : `/api/Categories`
- Tags : `/api/Tags`
- Émissions : `/api/Emissions`
- Équipe : `/api/Team`
- Statistiques : `/api/Dashboard`

Les routes d'administration utilisent les rôles JWT `Admin` et `Redacteur`. Les suppressions sont réservées à `Admin`.

## Production

Ne jamais stocker de mot de passe, clé JWT ou clé d'API dans le dépôt. Utiliser les variables d'environnement ASP.NET Core, notamment :

```text
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
Cors__AllowedOrigins__0
```
