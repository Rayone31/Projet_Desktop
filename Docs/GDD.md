# GDD - DMsound (V1.0)

Document construit a partir de Docs/UserStory.md et Docs/constitution.md.

## 1. Presentation du produit
DMsound est une application desktop Windows de soundboard orientee usage personnel, permettant de declencher rapidement des sons au clavier ou au clic, avec priorite a la stabilite audio.

## 2. Vision produit
Permettre a un utilisateur de jouer des extraits audio instantanement dans ses sessions vocales (Discord/jeux), avec un flux simple: importer, editer, assigner, jouer.

## 3. Objectifs
1. Declenchement rapide et fiable des sons.
2. Experience de configuration simple en fenetre unique.
3. Edition audio de base suffisante pour preparer des extraits utiles.
4. Persistance locale robuste des soundboards et raccourcis.
5. Respect strict des contraintes de qualite technique et de DA.

## 4. Public cible
1. Utilisateur principal: usage personnel general.
2. Contexte d'usage: vocal en ligne, jeux, discussions entre amis, animation de sessions.

## 5. Plateforme et distribution
1. Plateforme MVP: Windows uniquement.
2. Distribution MVP: installateur Windows classique.
3. Langue MVP: francais uniquement.

## 6. Perimetre fonctionnel

### 6.1 MVP (Must)
1. Ouvrir une soundboard et jouer des sons au clic.
2. Assigner une touche clavier a un son.
3. Garantir une touche unique par son, sans conflit autorise.
4. Detection automatique du micro virtuel pour la sortie audio.
5. Fallback si micro virtuel absent: alerte utilisateur et sortie par defaut conservee.
6. Import audio par glisser-deposer.
7. Formats minimum supportes: WAV, MP3.
8. Edition audio:
1. Visualisation waveform.
2. Selection debut/fin.
3. Pre-ecoute de la selection.
4. Decoupage non destructif avec creation d'une copie.

### 6.2 Post-MVP (Should)
1. Support de formats supplementaires.
2. Creation de plusieurs soundboards.
3. Renommage des soundboards.
4. Gestion de la visibilite des soundboards dans le menu.
5. Conservation des soundboards entre sessions.
6. Mode fenetre et plein ecran.
7. Ameliorations de confort UI.

### 6.3 Hors perimetre V1
1. Synchronisation cloud.
2. Collaboration multi-utilisateur.
3. Marketplace/partage integre.
4. Effets audio avances temps reel.

## 7. Flux utilisateur principaux

### 7.1 Jouer un son
1. Ouvrir la soundboard.
2. Cliquer un bouton ou appuyer sur la touche assignee.
3. Le son est joue immediatement.
4. Le son est route vers la sortie audio active.

### 7.2 Importer et preparer un son
1. Glisser-deposer un fichier audio.
2. Ouvrir l'editeur.
3. Visualiser la waveform.
4. Selectionner debut/fin.
5. Pre-ecouter la selection.
6. Valider le decoupage.
7. Enregistrer une copie exploitable dans la soundboard.

### 7.3 Gerer les soundboards
1. Creer une nouvelle soundboard.
2. Renommer si necessaire.
3. Choisir la visibilite dans le menu.
4. Retrouver les soundboards apres relance.

## 8. Regles metier
1. Une touche ne peut pas etre assignee a plusieurs sons dans la meme configuration active.
2. Le decoupage est non destructif par defaut.
3. Les donnees utilisateur sont sauvegardees localement en JSON.
4. Le nombre de sons par soundboard est illimite avec pagination.

## 9. Exigences non fonctionnelles

### 9.1 Performance et stabilite
1. Priorite absolue: stabilite.
2. Objectif UX: declenchement percu instantane.
3. Arbitrage officiel: si conflit perf/stabilite, la stabilite prime.

### 9.2 Qualite code et architecture
1. Langage: C#.
2. Clean Architecture: Domain, Application, Infrastructure, UI.
3. Domaine independant des frameworks.
4. TDD obligatoire: Red, Green, Refactor.
5. Limites de qualite:
1. 50 lignes max par fonction.
2. 120 colonnes max.
3. CCN max 5.
4. CRAP score max 25.

### 9.3 Processus et securite
1. GitFlow: une branche par feature.
2. Pas de push direct sur master.
3. Standard de commit obligatoire.
4. Aucun secret dans le depot (tokens/credentials).
5. Documentation maintenue au fil de l'eau dans Docs.
6. Compatibilite framework: 11.x.
7. Pas de regle explicite ajoutee sur les droits audio (decision actuelle: non).

## 10. UX et direction artistique
1. Interface en fenetre unique avec panneaux.
2. DA cible: futuriste 2D vectoriel avec touche gaming retro.
3. Interdiction d'introduire de nouvelles palettes, typos ou styles sans validation explicite.
4. Interface lisible, coherente, intuitive.

## 11. Architecture fonctionnelle (haut niveau)
1. Module Soundboard: affichage des sons, actions clic/clavier.
2. Module Audio Engine: lecture, routage sortie, gestion peripheriques.
3. Module Import: drag and drop, validation format.
4. Module Editeur: waveform, selection, pre-ecoute, decoupage.
5. Module Persistance: lecture/ecriture JSON locale.
6. Module UI Shell: navigation, etat UI, mode d'affichage.

## 12. Donnees et persistance
1. Entite Soundboard: id, nom, visibilite, liste sons.
2. Entite Son: id, nom, chemin source, chemin extrait, raccourci, metadonnees.
3. Entite Preferences: mode d'affichage, peripherique sortie prefere.
4. Stockage: JSON local.
5. Restauration automatique au lancement.

## 13. Metriques de succes MVP
1. Temps moyen de declenchement audio.
2. Taux d'import reussi.
3. Nombre d'erreurs de sortie audio virtuelle.
4. Nombre d'actions necessaires pour jouer un son.

## 14. Definition of Done
1. Tests unitaires verts.
2. Tests d'acceptation user stories verts.
3. Documentation mise a jour.
4. Revue de code validee.

## 15. Roadmap
1. Sprint 1 (2 semaines): lecture sons, soundboard active, raccourcis, sortie audio.
2. Sprint 2 (2 semaines): import WAV/MP3, persistance JSON de base.
3. Sprint 3 (2 semaines): edition audio complete non destructive.
4. Sprint 4 (2 semaines): multi-soundboards, visibilite, conservation.
5. Sprint 5 (2 semaines): optimisation UX/DA, hardening stabilite.

## 16. Risques et mitigations
1. Risque: variations de drivers audio Windows.
2. Mitigation: couche d'abstraction peripheriques + fallback explicite.
3. Risque: instabilite en sortie virtuelle.
4. Mitigation: detection robuste, logs techniques, retour utilisateur clair.
5. Risque: derive de complexite.
6. Mitigation: TDD strict + seuils qualite constitutionnels.

## 17. Tracabilite User Stories -> GDD
1. Feature 1 couverte en sections 6, 7, 8, 11.
2. Feature 2 couverte en sections 6, 7, 11.
3. Feature 3 couverte en sections 6, 7, 11.
4. Feature 4 couverte en sections 6, 7, 12.
5. Feature 5 couverte en sections 6, 10.
