## Résumé

<!-- Décrire brièvement le besoin traité et l'impact attendu -->

## Changements

- <!-- Changement 1 -->
- <!-- Changement 2 -->

## Validation

- [ ] `dotnet build Ludokino.sln --configuration Release`
- [ ] `dotnet test Ludokino.sln --configuration Release`
- [ ] `npm --prefix ludokino-web run lint`
- [ ] `npm --prefix ludokino-web run build`

## Checklist architecture & sécurité

- [ ] Le démarrage en production reste bloquant si JWT/CORS ne sont pas configurés
- [ ] Les contrôleurs ne dépendent pas directement de la persistance
- [ ] Les endpoints sensibles conservent une protection auth/authz explicite
- [ ] Les accès externes (origines CORS, URLs sortantes, headers) restent cohérents avec la politique de sécurité
- [ ] La documentation d'architecture/sécurité est mise à jour si un changement structurant est introduit
