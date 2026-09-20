# Tests de l'API Ludokino

Ce projet contient les tests unitaires des services, les tests directs des contrôleurs et le test d'intégration du schéma PostgreSQL.

## Lancer les tests

```powershell
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj --configuration Release
```

## Tests unitaires

Les tests des services utilisent EF Core InMemory. Ils couvrent notamment :

- l'authentification et les mots de passe invalides;
- la génération de slugs d'articles;
- l'exclusion des brouillons;
- la validation des liens YouTube des émissions;
- les réponses HTTP des contrôleurs;
- les headers de sécurité en développement et en production.

Ils ne nécessitent pas de serveur PostgreSQL.

## Test PostgreSQL

Le test de schéma utilise une base séparée nommée `ludokino_test` et les mêmes migrations EF Core que l'API. Il ne doit jamais utiliser `ludokino` ou une base de production.

```powershell
$env:LUDOKINO_TEST_CONNECTION_STRING = "Host=localhost;Database=ludokino_test;Username=postgres;Password=<mot-de-passe>"
dotnet test Ludokino.Api.Tests/Ludokino.Api.Tests.csproj --configuration Release
Remove-Item Env:LUDOKINO_TEST_CONNECTION_STRING
```

Le test applique les migrations, vérifie la connexion et confirme qu'aucune migration ne reste en attente. Sans la variable d'environnement, il est ignoré.

## CI

GitHub Actions démarre automatiquement un service PostgreSQL 16 et fournit `LUDOKINO_TEST_CONNECTION_STRING` pendant le job `build-and-test`. Le test d'intégration est donc exécuté dans chaque Pull Request.
