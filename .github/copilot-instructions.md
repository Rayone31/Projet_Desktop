Project Guidelines

## Priorités non négociables

  Produire un code professionnel, maintenable et lisible ; éviter le « code moche ».
  Ne jamais exposer de secrets (tokens, credentials) dans le dépôt, les logs, les exemples ou les tests.
  Appliquer la Clean Architecture (séparation Domain/Application/Infrastructure/UI) avec des dépendances orientées vers l’intérieur.
  Prioriser la stabilité avant l'ajout de nouvelles fonctionnalités.
  TDD obligatoire (Red → Green → Refactor) pour toute nouvelle logique.

## Style de code

  Fonctions : 50 lignes max ; préférer plusieurs petites fonctions.
  Largeur : 120 colonnes max.
  Complexité : CCN ≤ 5 ; refactorer dès que ça dépasse.
  CRAP score ≤ 25 : réduire la complexité et renforcer les tests.
  Nommage explicite ; éviter les abréviations inutiles.
  Pas d’emojis dans le code.

## Architecture

Garder le domaine indépendant des frameworks et des entrées/sorties (I/O).
Utiliser des ports/adapters et l’injection de dépendances quand nécessaire.
Favoriser du code facilement testable (petites unités, fonctions pures quand possible).
Garder une architecture VSM (View-Services-Model) pour la partie UI, avec une séparation claire entre la logique métier (Services) et la présentation (View/Model).
Utiliser NAudio comme librairie audio de référence pour la lecture, le routage et le traitement audio.
Implémenter la logique applicative uniquement en C#.

## Build & tests

Écrire/mettre à jour les tests avant de livrer une feature (TDD).
Quand une commande ou un outil d’automatisation est nécessaire, privilégier des scripts C#/.NET.

## Git & commits

Branches du projet : master, audio, reseau.
master est la branche principale de développement et d’intégration.
Tout merge vers master doit passer par PR et rebase préalable sur master.
Interdiction de push direct sur master.
Travail en parallèle :
  audio pour le stream Audio/Soundboard.
  reseau pour le stream Réseau/Session.
Commits : respecter un standard (ex. Conventional Commits : feat:, fix:, chore:, etc.).

## Organisation équipe

Répartition des responsabilités :
  Audio/Soundboard : owner de US 1.x, 2.x, 3.x, 4.x.
  Réseau/Session : owner de US 6.x.
  Zone partagée : US 5.x et modèles Domain communs.

Coordination obligatoire :
  Synchronisation technique minimum 2 fois par semaine.
  Toute modification cassante d’interface doit être annoncée avant merge.
  Les changements sur Domain/UI partagé demandent validation des deux développeurs.

## Documentation

Documenter au fil de l’eau (dossier Docs/).
S’appuyer sur la documentation officielle des frameworks ; ne pas inventer d’API.
Framework : rester compatible avec la version 11.x (pas de montée de version majeure non demandée).

## UI / DA

Respecter la DA : thème futuriste 2D vectoriel.
Ne pas introduire de nouvelles palettes/typos/styles hors de la DA existante sans demande explicite.