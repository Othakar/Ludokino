# Ludokino.Api.Tests

Projet de tests .NET 8 utilisant xUnit, EF Core InMemory et PostgreSQL.

## Exécuter les tests

Depuis la racine :

```powershell
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj --configuration Release
```

Ou sur toute la solution :

```powershell
dotnet test Ludokino.sln --configuration Release
```

## Types de tests

- Tests de services avec EF Core InMemory.
- Tests directs des contrôleurs et des réponses HTTP.
- Tests des permissions déclarées sur les contrôleurs.
- Tests des headers de sécurité de l'API.
- Test d'intégration du schéma PostgreSQL et des migrations.

## PostgreSQL

Le test d'intégration utilise exclusivement la base `ludokino_test` :

```powershell
$env:LUDOKINO_TEST_CONNECTION_STRING = "Host=localhost;Database=ludokino_test;Username=postgres;Password=<mot-de-passe>"
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj --configuration Release
Remove-Item Env:LUDOKINO_TEST_CONNECTION_STRING
```

Le test applique les migrations et vérifie qu'il ne reste aucune migration en attente. Sans cette variable, le test PostgreSQL est ignoré.

## CI

Le workflow GitHub Actions démarre PostgreSQL 16 dans un service éphémère et définit automatiquement `LUDOKINO_TEST_CONNECTION_STRING`. Les tests d'intégration sont donc exécutés dans les Pull Requests.

## Conventions

Les tests ne doivent jamais utiliser la base de développement ou une base de production. Les secrets doivent être fournis par variables d'environnement et ne doivent pas être commités.
