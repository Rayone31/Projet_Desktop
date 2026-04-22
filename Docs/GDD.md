# GDD - DMsound 

Document aligne sur Docs/UserStory.md et Docs/constitution.md.

## 1. Presentation du produit
DMsound est une application desktop Windows de soundboard permettant de jouer et partager des sons localement et en session reseau.

## 2. Vision produit
Offrir un outil simple et fiable pour:
- importer et preparer des extraits audio,
- les declencher instantanement,
- les diffuser localement ou en session partagee.

## 3. Objectifs produit
1. Lecture audio immediate au clic et au clavier.
2. Configuration rapide des sons dans une interface simple.
3. Edition audio utile (selection, pre-ecoute, decoupage).
4. Sauvegarde et restauration des soundboards.
5. Partage reseau en temps reel via session codee.
6. Stabilite prioritaire sur toute extension de scope.

## 4. Public cible
1. Utilisateur principal: gamer, streamer, animateur vocal, usage perso.
2. Contextes: Discord, jeu en ligne, animation de groupe.

## 5. Plateforme et langue
1. Plateforme MVP: Windows.
2. Distribution MVP: desktop.
3. Langue MVP: francais.
4. Stack technique imposee: C#/.NET et WPF.
5. Librairie audio imposee: NAudio.

## 6. Perimetre fonctionnel

### 6.1 MVP (Must)
1. Ouvrir une soundboard et jouer un son (US 1.1, 1.2).
2. Assigner et declencher des raccourcis clavier (US 1.3).
3. Sortie audio via micro virtuel si disponible (US 1.4).
4. Import glisser-deposer des fichiers audio (US 2.1).
5. Edition audio complete:
- waveform,
- selection debut/fin,
- pre-ecoute,
- decoupage (US 3.1 a 3.4).
6. Reseau session:
- creer une session,
- rejoindre via code,
- jouer un son entendu par tous,
- quitter une session (US 6.1, 6.2, 6.3, 6.5).

### 6.2 Post-MVP (Should)
1. Formats audio supplementaires (US 2.2).
2. Gestion avancee des soundboards:
- creer,
- renommer,
- visibilite menu,
- conservation (US 4.1 a 4.4).
3. Ameliorations UI:
- interface simplifiee,
- mode fenetre/plein ecran,
- design (US 5.1 a 5.3).
4. Reseau comfort:
- liste des membres,
- fermeture session par l'hote (US 6.4, 6.6).

### 6.3 Hors perimetre V2
1. Marketplace de sons.
2. Effets audio avances temps reel.
3. Cloud sync multi-device.

## 7. Regles metier
1. Un son est declenchable par bouton et/ou touche.
2. Une touche ne doit pas etre en conflit dans la meme soundboard active.
3. Une session reseau est identifiee par un code unique.
4. Seul l'hote peut fermer la session pour tous.
5. Les soundboards existantes sont conservees localement.

## 8. Exigences non fonctionnelles

### 8.1 Performance et stabilite
1. Stabilite prioritaire.
2. Declenchement percu instantane.
3. Diffusion reseau stable pour usage temps reel.

### 8.2 Architecture et qualite
1. Langage: C#.
2. Application et logique metier: uniquement en C# (aucun autre langage en production).
3. Audio I/O, playback, routage et edition: implementation via NAudio.
4. Architecture: Clean Architecture (Domain, Application, Infrastructure, UI).
5. TDD obligatoire.
6. Limites de code:
- 50 lignes max/fonction,
- 120 colonnes max,
- CCN max 5,
- CRAP max 25.

### 8.3 Securite et processus
1. Pas de secrets dans le depot.
2. GitFlow et PR obligatoires.
3. Documentation maintenue dans Docs/.

## 9. Architecture fonctionnelle (haut niveau)
1. UI Shell: navigation generale et etat de l'application.
2. Soundboard Module: liste des sons, interactions utilisateur.
3. Audio Engine: lecture locale et routage output avec NAudio.
4. Audio Editor: waveform, selection, pre-ecoute, decoupage.
5. Import Module: drag and drop et validation formats.
6. Persistence Module: sauvegarde/restauration locale.
7. Session Module: creation/rejoindre/quitter session et diffusion audio.

## 10. Donnees principales
1. Soundboard: id, nom, visible, liste sons.
2. Son: id, nom, chemin fichier, raccourci, meta.
3. Session: code, hote, membres, statut.
4. Preferences: output audio prefere, affichage UI.

## 11. Definition of Done
1. Criteres Given/When/Then valides.
2. Tests unitaires verts.
3. Tests integration critiques verts.
4. Review de code validee.
5. Documentation mise a jour.

## 12. Traceabilite stories -> sections
1. Feature 1: sections 6.1, 7, 9.
2. Feature 2: sections 6.1/6.2, 9.
3. Feature 3: sections 6.1, 9.
4. Feature 4: sections 6.2, 10.
5. Feature 5: sections 6.2, 9.
6. Feature 6: sections 6.1/6.2, 7, 9.

## 13. Contrats inter-modules Audio et Session
1. L'equipe Audio expose des interfaces Application stables pour lecture et etat sonore.
2. L'equipe Reseau consomme ces interfaces sans connaitre NAudio ni l'Infrastructure Audio.
3. Les evenements de session sont mappes vers des commandes Application (play, stop, sync etat).
4. Les modeles partages (SoundId, SessionId, UserId) sont versionnes et valides en double review.

### 13.1 Frontieres techniques
1. Audio Infrastructure (NAudio) reste encapsulee derriere Application.
2. Session Infrastructure (transport reseau) reste encapsulee derriere Application.
3. Aucune dependance Infrastructure -> Infrastructure entre Audio et Session.

### 13.2 Cadence d'integration
1. Un point d'integration fonctionnelle est planifie chaque fin de sprint.
2. Les regressions cross-modules bloquent le merge vers master.
