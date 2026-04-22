# Plan de developpement - DMsound

Document aligne sur Docs/GDD.md, Docs/UserStory.md et Docs/constitution.md.

## 1. Vue d'ensemble

- Produit: DMsound.
- Objectif: livrer un MVP Windows stable en 5 sprints.
- Equipe cible: 2 a 3 developpeurs.
- Cadence: 5 sprints de 2 semaines (10 semaines).
- Priorite strategique: stabilite avant extension de scope.
- Approche qualite: TDD obligatoire + quality gates.

## 2. Contraintes de cadrage

- Plateforme MVP: Windows uniquement.
- Distribution MVP: installateur Windows classique.
- Langue MVP: francais.
- Formats MVP: WAV, MP3.
- Persistance locale: JSON.
- UI: fenetre unique avec panneaux.
- Edition audio: non destructive (creation de copie).
- Raccourcis: touche unique par son, conflits interdits.
- Should features: post-MVP, sauf si capacite restante sans risque sur la stabilite.

## 3. Ordonnancement des phases

### 3.1 Vue dependances

1. Sprint 1 - Coeur audio (bloquant)
2. Sprint 2 - Import et persistance (depends on Sprint 1)
3. Sprint 3 - Edition audio (depends on Sprint 2)
4. Sprint 4 - Gestion soundboards (depends on Sprint 2)
5. Sprint 5 - UI polish et stabilisation finale (depends on Sprints 1-4)

### 3.2 Jalons critiques

- Jalon A (fin Sprint 1): lecture audio fiable + sortie audio fonctionnelle.
- Jalon B (fin Sprint 2): import WAV/MP3 + sauvegarde/restauration JSON.
- Jalon C (fin Sprint 3): pipeline edition complet non destructif.
- Jalon D (fin Sprint 4): gestion multi-soundboards operationnelle.
- Jalon E (fin Sprint 5): release candidate stable + documentation complete.

## 4. Backlog par sprint

## Sprint 1 - Audio Core (Must)

Objectif sprint:
- Poser le socle de lecture audio stable et pilotable par UI/clavier.

Stories et livrables:
1. US 1.1 - Jouer un son
- Livrable: declenchement immediat au clic.
- AC cible: clic bouton -> son joue immediatement.
2. US 1.3 - Raccourcis clavier
- Livrable: assignation et declenchement clavier.
- AC cible: touche assignee -> son joue.
- Regle: unicite des touches.
3. US 1.4 - Sortie audio systeme
- Livrable: detection auto micro virtuel + fallback.
- AC cible: sortie sur micro virtuel si detecte, sinon alerte et sortie par defaut.

Criteres de sortie sprint:
- Pipeline audio operationnel de bout en bout.
- Tests unitaires et tests d'integration audio verts.
- Logs minimaux pour diagnostics audio.

## Sprint 2 - Import et persistance (Must)

Objectif sprint:
- Permettre l'alimentation de la soundboard et la sauvegarde fiable.

Stories et livrables:
1. US 2.1 - Import glisser-deposer
- Livrable: import multi-fichiers depuis UI.
- AC cible: fichiers importes visibles en soundboard.
2. US 2.2 - Formats supportes (portion MVP)
- Livrable: validation WAV/MP3.
- AC cible: WAV/MP3 acceptes et jouables.
3. Persistance locale JSON
- Livrable: sauvegarde/restauration soundboards, sons, raccourcis, preferences UI.
- AC cible: etat restaure apres relance.

Criteres de sortie sprint:
- Flux import -> lecture stable valide.
- Donnees persistantes sans corruption sur scenarios standards.

## Sprint 3 - Edition audio (Must)

Objectif sprint:
- Fournir l'edition utile a la creation d'extraits exploitables.

Stories et livrables:
1. US 3.1 - Visualisation waveform
- Livrable: affichage de la forme d'onde.
2. US 3.2 - Selection audio
- Livrable: points debut/fin et plage visible.
3. US 3.3 - Pre-ecoute
- Livrable: lecture limitee a la selection.
4. US 3.4 - Decoupage audio
- Livrable: export d'une copie decoupee.
- Regle: original conserve.

Criteres de sortie sprint:
- Pipeline edition complet valide sur WAV/MP3.
- Contrats d'edition testes (bornes, selection, non-destruction).

## Sprint 4 - Gestion des soundboards (Should)

Objectif sprint:
- Structurer l'organisation utilisateur sans compromettre la stabilite.

Stories et livrables:
1. US 4.1 - Creer une soundboard
2. US 4.2 - Renommer une soundboard
3. US 4.3 - Visibilite dans le menu
4. US 4.4 - Conservation des soundboards

Criteres de sortie sprint:
- Parcours de gestion complet create/rename/hide/restore.
- Persistence robuste sur redemarrage.

## Sprint 5 - UX, affichage et stabilisation (Should)

Objectif sprint:
- Finaliser l'experience utilisateur et verrouiller la release.

Stories et livrables:
1. US 5.1 - Interface simple
- Livrable: navigation en fenetre unique, actions essentielles accessibles.
2. US 5.2 - Mode d'affichage
- Livrable: bascule fenetre/plein ecran.
3. US 5.3 - Design
- Livrable: DA futuriste 2D vectoriel, lisible et coherente.
4. Stabilisation finale
- Livrable: reduction anomalies critiques, optimisation robustesse.

Criteres de sortie sprint:
- Release candidate stable.
- Documentation finalisee.

## 5. Strategie TDD et tests

## 5.1 Workflow TDD obligatoire

Pour chaque user story:
1. Red: ecrire les tests d'acceptation/contrat qui echouent.
2. Green: implementer le minimum pour passer.
3. Refactor: ameliorer structure et lisibilite sans casser les tests.

## 5.2 Pyramide de tests

- Tests unitaires:
- Domaine et cas d'usage (regles d'unicite touches, validation formats, logique de selection).
- Tests d'integration:
- Pipeline audio, import -> lecture, persistance JSON, detection output.
- Tests d'acceptation:
- Scenarios Given/When/Then aligns aux criteres des user stories.

## 5.3 Matrice de couverture minimale

- US 1.x: unitaires + integration audio + acceptance.
- US 2.x: unitaires import/validation + integration I/O + acceptance.
- US 3.x: unitaires edition + integration rendu/lecture + acceptance.
- US 4.x: unitaires gestion board + integration persistance + acceptance.
- US 5.x: integration UI + acceptance parcours.

## 6. CI minimale obligatoire

Pipeline CI a activer des Sprint 1:
1. Build C#.
2. Execution tests unitaires.
3. Execution tests d'integration critiques.
4. Publication rapport de tests.
5. Verification quality gates.

Politique de merge:
- Pas de merge si pipeline rouge.
- Pas de contournement des tests critiques.

## 7. Quality gates

Regles bloquantes a chaque merge:
1. Fonction <= 50 lignes.
2. Ligne <= 120 colonnes.
3. CCN <= 5.
4. CRAP <= 25.
5. Pas de secrets dans le depot.
6. Documentation mise a jour sur changements de comportement.

## 8. Organisation equipe (2-3 devs)

- Dev A: audio engine, output routing, stabilite.
- Dev B: import/persistance, gestion soundboards.
- Dev C (si present): UI shell, editeur audio, UX.

Rituels:
- Daily court.
- Review PR obligatoire.
- Demo fin de sprint.
- Retro orientee qualite/stabilite.

## 9. Definition of Done

Une story est terminee si:
1. Critere d'acceptation valide.
2. Tests unitaires verts.
3. Tests d'acceptation verts.
4. Review de code approuvee.
5. Documentation impactee mise a jour.
6. Aucun quality gate viole.

## 10. Gestion des risques

## 10.1 Registre priorise

1. Risque: variabilite drivers audio Windows.
- Severite: haute.
- Mitigation: abstraction peripheriques + tests integration multi-config.
2. Risque: echec detection micro virtuel.
- Severite: haute.
- Mitigation: fallback explicite + alerte utilisateur + logs diagnostic.
3. Risque: instabilite sous charge (lectures rapides/enchainements).
- Severite: haute.
- Mitigation: scenarios de stress en integration + priorite stabilite.
4. Risque: corruption JSON.
- Severite: moyenne.
- Mitigation: validation schema + sauvegarde atomique + recovery minimal.
5. Risque: derive de complexite code.
- Severite: moyenne.
- Mitigation: TDD strict + quality gates + refactor continu.

## 10.2 Escalade

- Bloquant release: toute regression sur audio core, persistance ou lancement app.
- Arbitrage: reduction scope Should avant tout compromis qualite.

## 11. Metriques de pilotage

Metriques MVP suivies chaque sprint:
1. Temps moyen de declenchement audio.
2. Taux d'import reussi WAV/MP3.
3. Nombre d'erreurs de sortie audio virtuelle.
4. Nombre d'actions pour jouer un son.
5. Taux de tests verts en CI.

## 12. Traceabilite User Stories -> Sprints -> Tests

- US 1.1 -> Sprint 1 -> unitaires + integration audio + acceptance.
- US 1.2 -> Sprint 1/2 (ouverture board dans shell) -> integration UI + acceptance.
- US 1.3 -> Sprint 1 -> unitaires raccourcis + acceptance.
- US 1.4 -> Sprint 1 -> integration output + acceptance fallback.
- US 2.1 -> Sprint 2 -> unitaires import + integration I/O + acceptance.
- US 2.2 -> Sprint 2 -> unitaires validation format + acceptance.
- US 3.1 -> Sprint 3 -> integration waveform + acceptance.
- US 3.2 -> Sprint 3 -> unitaires selection + acceptance.
- US 3.3 -> Sprint 3 -> integration playback selection + acceptance.
- US 3.4 -> Sprint 3 -> unitaires decoupage + acceptance non destructif.
- US 4.1 -> Sprint 4 -> unitaires gestion board + acceptance.
- US 4.2 -> Sprint 4 -> unitaires renommage + acceptance.
- US 4.3 -> Sprint 4 -> unitaires visibilite + acceptance.
- US 4.4 -> Sprint 4 -> integration persistance + acceptance relance.
- US 5.1 -> Sprint 5 -> integration parcours UI + acceptance.
- US 5.2 -> Sprint 5 -> integration affichage + acceptance.
- US 5.3 -> Sprint 5 -> revue DA + acceptance lisibilite/coherence.

## 13. Perimetre explicite

Inclus MVP:
- Toutes les stories Must.

Post-MVP:
- Stories Should, planifiees Sprints 4-5 selon capacite.

Exclusions actuelles:
- Synchronisation cloud.
- Collaboration multi-utilisateur.
- Marketplace integree.
- Effets audio avances temps reel.

## 14. References

- Docs/GDD.md
- Docs/UserStory.md
- Docs/constitution.md
