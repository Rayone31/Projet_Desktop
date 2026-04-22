# Plan de developpement - DMsound (V3)

Document aligne sur Docs/UserStory.md, Docs/GDD.md et Docs/constitution.md.

## 1. Strategie globale
1. Priorite absolue: livrer toutes les stories Must.
2. Travail en parallele par deux streams: Audio/Soundboard et Reseau/Session.
3. Cadence proposee: 6 sprints de 2 semaines.
4. Methode: TDD systematique + quality gates a chaque merge.
5. Contrainte techno: application uniquement en C# et pipeline audio base sur NAudio.

## 2. Repartition des streams

### 2.1 Stream Audio/Soundboard (Dev A)
1. US 1.1, 1.2, 1.3, 1.4.
2. US 2.1, 2.2.
3. US 3.1, 3.2, 3.3, 3.4.
4. US 4.1, 4.2, 4.3, 4.4.

### 2.2 Stream Reseau/Session (Dev B)
1. US 6.1, 6.2, 6.3, 6.4, 6.5, 6.6.

### 2.3 Zone partagee
1. US 5.1, 5.2, 5.3.
2. Modeles Domain partages.
3. UI Shell et integration finale.

## 3. Planning parallele par sprint

## Sprint 1
1. Audio: US 1.1 a 1.4 operationnelles.
2. Reseau: cadrage protocole session et contrats d'integration.
3. Sync: stabiliser les interfaces Application cibles pour l'appel Audio depuis Session.

## Sprint 2
1. Audio: US 2.1 et US 2.2 MVP.
2. Reseau: implementation US 6.1 et US 6.2.
3. Sync: test integration creation session avec catalogue sons charge.

## Sprint 3
1. Audio: US 3.1 a 3.4.
2. Reseau: implementation US 6.3 (jouer un son en session).
3. Sync: test cross-team diffusion son en session via interfaces stables.

## Sprint 4
1. Audio: US 4.1 a 4.4.
2. Reseau: implementation US 6.5 et US 6.4.
3. Sync: validation persistance locale + etat session coherent.

## Sprint 5
1. Audio: stabilisation et reduction dette technique.
2. Reseau: implementation US 6.6 et hardening transport.
3. Sync: branche integration temporaire pour fusionner les deux streams.

## Sprint 6
1. Audio + Reseau: US 5.1, 5.2, 5.3.
2. Sync: final QA, correction regressions, preparation release candidate.

## 4. Strategie Git et integration
1. Branches de travail: audio et reseau.
2. Branche principale d'integration: master.
3. Merge vers master uniquement via Pull Request.
4. Rebase sur master avant chaque PR.
5. Point de merge coordonne 2 fois par semaine minimum.

## 5. Quality gates
1. Fonction <= 50 lignes.
2. Ligne <= 120 colonnes.
3. CCN <= 5.
4. CRAP <= 25.
5. Aucune fuite de secrets.
6. Aucun code applicatif hors C#.
7. Toute fonctionnalite audio doit passer par NAudio.

## 6. Strategie de tests
1. Unitaires: regles metier, mapping commandes, etats de session.
2. Integration: pipeline audio local, persistance, transport reseau.
3. Acceptation: scenarios Given/When/Then de UserStory.
4. Cross-team: tests d'integration Audio + Session obligatoires des Sprint 3.

## 7. Definition of Done
1. Critere d'acceptation valide.
2. Tests unitaires verts.
3. Tests integration critiques verts.
4. Revue de code approuvee.
5. Documentation mise a jour.

## 8. Risques de parallelisation et mitigations
1. Risque: conflit sur modeles partages.
- Mitigation: ownership clair + double review obligatoire.
2. Risque: divergence des branches feature.
- Mitigation: sync hebdomadaire + rebase avant PR.
3. Risque: regression integration Audio/Session.
- Mitigation: PR courtes et frequentes vers master + tests cross-team.
4. Risque: conflit UI sur US 5.x.
- Mitigation: planification conjointe et decoupage des zones UI.

## 9. Traceabilite stories -> stream
1. US 1.x, 2.x, 3.x, 4.x -> Stream Audio/Soundboard.
2. US 6.x -> Stream Reseau/Session.
3. US 5.x -> Stream partage.
