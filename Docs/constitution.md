# Constitution du projet

## Priorites non negociables
- Produire un code professionnel, maintenable et lisible.
- Git obligatoire.
- Ne jamais exposer de secrets (credentials, tokens) sur Git.
- Appliquer la Clean Architecture.
- TDD obligatoire (Red -> Green -> Refactor).

## Style de code
- Fonctions: 50 lignes maximum.
- Largeur de ligne: 120 colonnes maximum.
- Complexite cyclomatique (CCN): 5 maximum.
- CRAP score: 25 maximum.
- Nommage explicite.
- Pas d'emojis partout dans le code.

## Architecture
- Clean Architecture: separer Domain, Application, Infrastructure et UI.
- Garder le domaine independant des frameworks.
- S'appuyer sur la documentation officielle des frameworks.

## Build et tests
- Ecrire les tests avant la livraison d'une feature (TDD).
- Quand une automatisation est necessaire, privilegier des scripts Python.

## Git et commits
- Une branche par feature (GitFlow).
- Interdiction de push direct sur master.
- Respecter les standards de commit.

## Documentation
- Faire la documentation au fur et a mesure.
- Maintenir la documentation dans le dossier Docs/.
- Conserver la compatibilite framework en version 11.x.

## UI / DA
- Respecter la DA.
- DA: theme futuriste 2D vectoriel.
- Ne pas introduire de nouvelles palettes, typos ou styles hors de la DA existante sans demande explicite.

## Contraintes techniques
- Langage de code: C#.
