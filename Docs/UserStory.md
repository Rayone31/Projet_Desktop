# User Stories - Projet Soundboard

## Feature 1 - Lecture de sons

### User Story 1.1 - Jouer un son
En tant qu'utilisateur,  
Je veux declencher un son via un bouton,  
Afin de jouer un extrait audio.

- Priorite: Must
- Criteres d'acceptation:

Given une soundboard contenant des sons  
When je clique sur un bouton de son  
Then le son associe est joue immediatement

### User Story 1.2 - Lancer une soundboard
En tant qu'utilisateur,  
Je veux acceder a une soundboard,  
Afin d'utiliser ses sons.

- Priorite: Must
- Criteres d'acceptation:

Given plusieurs soundboards configurees  
When je selectionne une soundboard  
Then la soundboard selectionnee est ouverte et ses sons sont utilisables

### User Story 1.3 - Raccourcis clavier
En tant qu'utilisateur,  
Je veux assigner une touche a un son,  
Afin de le declencher rapidement.

- Priorite: Must
- Criteres d'acceptation:

Given un son configure avec une touche  
When j'appuie sur la touche assignee  
Then le son associe est joue

### User Story 1.4 - Sortie audio systeme
En tant qu'utilisateur,  
Je veux que les sons passent par mon micro virtuel,  
Afin de les partager sur Discord et en jeu.

- Priorite: Must
- Criteres d'acceptation:

Given un micro virtuel selectionne comme sortie  
When je joue un son  
Then le son est emis sur la sortie audio virtuelle

## Feature 2 - Gestion des fichiers audio

### User Story 2.1 - Import de fichiers
En tant qu'utilisateur,  
Je veux glisser-deposer des fichiers audio,  
Afin de les ajouter a ma soundboard.

- Priorite: Must
- Criteres d'acceptation:

Given l'interface de la soundboard est ouverte  
When je glisse-depose un ou plusieurs fichiers audio  
Then les fichiers sont importes et apparaissent dans la soundboard

### User Story 2.2 - Formats supportes
En tant qu'utilisateur,  
Je veux importer plusieurs formats audio,  
Afin d'utiliser differents types de fichiers.

- Priorite: Should
- Criteres d'acceptation:

Given des fichiers audio de formats differents  
When je les importe dans l'application  
Then les formats supportes sont acceptes et utilisables

## Feature 3 - Edition audio

### User Story 3.1 - Visualisation waveform
En tant qu'utilisateur,  
Je veux voir la forme d'onde,  
Afin de comprendre le son.

- Priorite: Must
- Criteres d'acceptation:

Given un fichier audio charge  
When j'ouvre l'editeur audio  
Then la forme d'onde du fichier est affichee

### User Story 3.2 - Selection audio
En tant qu'utilisateur,  
Je veux definir un debut et une fin,  
Afin de decouper un extrait.

- Priorite: Must
- Criteres d'acceptation:

Given un fichier audio charge dans l'editeur  
When je positionne un point de debut et un point de fin  
Then la plage selectionnee est clairement visible

### User Story 3.3 - Pre-ecoute
En tant qu'utilisateur,  
Je veux ecouter l'extrait selectionne,  
Afin de valider mon decoupage.

- Priorite: Must
- Criteres d'acceptation:

Given une plage audio selectionnee  
When je lance la pre-ecoute  
Then seule la plage selectionnee est lue

### User Story 3.4 - Decoupage audio
En tant qu'utilisateur,  
Je veux modifier un fichier audio,  
Afin de ne garder que la partie utile.

- Priorite: Must
- Criteres d'acceptation:

Given une selection audio valide  
When je confirme le decoupage  
Then le fichier est tronque a la plage selectionnee

## Feature 4 - Gestion des soundboards

### User Story 4.1 - Creer une soundboard
En tant qu'utilisateur,  
Je veux creer plusieurs soundboards,  
Afin d'organiser mes sons.

- Priorite: Should
- Criteres d'acceptation:

Given l'ecran de gestion des soundboards  
When je cree une nouvelle soundboard  
Then elle est ajoutee a la liste des soundboards

### User Story 4.2 - Renommer une soundboard
En tant qu'utilisateur,  
Je veux renommer une soundboard,  
Afin de mieux m'organiser.

- Priorite: Should
- Criteres d'acceptation:

Given une soundboard existante  
When je modifie son nom  
Then le nouveau nom est enregistre et affiche

### User Story 4.3 - Visibilite dans le menu
En tant qu'utilisateur,  
Je veux choisir quelles soundboards sont visibles,  
Afin de simplifier l'interface.

- Priorite: Should
- Criteres d'acceptation:

Given plusieurs soundboards disponibles  
When je masque une soundboard  
Then elle n'apparait plus dans le menu principal

### User Story 4.4 - Conservation des soundboards
En tant qu'utilisateur,  
Je veux garder mes anciennes soundboards,  
Afin de les reutiliser plus tard.

- Priorite: Should
- Criteres d'acceptation:

Given des soundboards existantes  
When je ferme puis relance l'application  
Then mes soundboards precedentes sont conservees

## Feature 5 - Interface utilisateur

### User Story 5.1 - Interface simple
En tant qu'utilisateur,  
Je veux une application avec peu de fenetres,  
Afin de faciliter l'usage.

- Priorite: Should
- Criteres d'acceptation:

Given l'application est lancee  
When je navigue entre les principales fonctions  
Then les actions essentielles sont accessibles sans multiplication de fenetres

### User Story 5.2 - Mode d'affichage
En tant qu'utilisateur,  
Je veux choisir entre fenetre et plein ecran,  
Afin d'adapter l'affichage.

- Priorite: Should
- Criteres d'acceptation:

Given l'application est ouverte  
When je change le mode d'affichage  
Then l'interface bascule correctement entre fenetre et plein ecran

### User Story 5.3 - Design
En tant qu'utilisateur,  
Je veux une interface agreable,  
Afin d'ameliorer le confort d'utilisation.

- Priorite: Should
- Criteres d'acceptation:

Given l'utilisateur utilise l'application  
When il interagit avec l'interface  
Then l'interface reste lisible, coherente et intuitive

## Feature 6 - Reseau

### User Story 6.1 - Creer une session
En tant qu'utilisateur,  
Je veux creer une session en ligne,  
Afin de partager mes sons avec d'autres personnes.

- Priorite: Must
- Criteres d'acceptation:

Given l'application est ouverte  
When je cree une nouvelle session  
Then un code unique est genere et affiche, et je deviens l'hote de la session

### User Story 6.2 - Rejoindre une session
En tant qu'utilisateur,  
Je veux rejoindre une session existante via un code,  
Afin d'acceder a la session partagee.

- Priorite: Must
- Criteres d'acceptation:

Given un code de session valide  
When je saisis ce code et confirme  
Then je rejoins la session et les autres membres sont notifies de mon arrivee

### User Story 6.3 - Jouer un son en session
En tant qu'utilisateur,  
Je veux jouer un son depuis ma soundboard pendant une session,  
Afin que tous les membres l'entendent en temps reel.

- Priorite: Must
- Criteres d'acceptation:

Given je suis connecte a une session active  
When je declenche un son depuis ma soundboard  
Then le son est diffuse et entendu simultanement par tous les membres de la session

### User Story 6.4 - Liste des membres
En tant qu'utilisateur,  
Je veux voir qui est connecte a la session,  
Afin de savoir qui participe.

- Priorite: Should
- Criteres d'acceptation:

Given une session active avec plusieurs membres  
When je consulte la session  
Then la liste des membres connectes est affichee et mise a jour en temps reel

### User Story 6.5 - Quitter une session
En tant qu'utilisateur,  
Je veux quitter une session,  
Afin de mettre fin a ma participation.

- Priorite: Must
- Criteres d'acceptation:

Given je suis connecte a une session active  
When je quitte la session  
Then je suis deconnecte et les autres membres sont notifies de mon depart

### User Story 6.6 - Fermeture de session par l'hote
En tant qu'hote,  
Je veux pouvoir fermer la session,  
Afin de mettre fin a la session pour tout le monde.

- Priorite: Should
- Criteres d'acceptation:

Given je suis l'hote d'une session active  
When je ferme la session  
Then tous les membres sont deconnectes et la session est supprimee