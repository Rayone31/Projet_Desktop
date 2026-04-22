# Constitution du projet DMsound

## 1. Principes non negociables
1. Stabilite avant ajout de fonctionnalites.
2. Code maintenable, lisible, teste.
3. Aucune fuite de secrets dans le depot.
4. Documentation tenue a jour dans Docs/.

## 2. Architecture et techniques
1. Langage principal: C#.
2. Architecture imposee: Clean Architecture.
3. Separation stricte: Domain, Application, Infrastructure, UI.
4. Domaine independant des frameworks.
5. Librairie audio obligatoire: NAudio.
6. Code applicatif uniquement en C# (hors fichiers de configuration standard).

## 3. Standards de code
1. Fonction <= 50 lignes.
2. Ligne <= 120 colonnes.
3. Complexite cyclomatique <= 5.
4. CRAP score <= 25.
5. Nommage explicite obligatoire.

## 4. Tests et qualite
1. TDD obligatoire: Red -> Green -> Refactor.
2. Build et tests obligatoires avant merge.
3. Aucun contournement des tests critiques.

## 5. Git et livraison
1. GitFlow: une branche par feature.
2. Pas de push direct sur master.
3. Pull Request obligatoire.
4. Revue de code obligatoire.

## 6. Regles produit (issues du UserStory)
1. Priorite de livraison:
- d'abord toutes les stories Must,
- ensuite les stories Should.
2. Feature reseau (US 6.x) fait partie du coeur fonctionnel Must pour:
- creer session,
- rejoindre session,
- diffuser son en session,
- quitter session.
3. Les contraintes audio et session doivent etre documentees et testees.

## 7. UX et DA
1. UI lisible, coherente et intuitive.
2. Respect de la DA existante du projet.
3. Pas de changement majeur de palette, typo ou style sans validation explicite.

## 8. Documentation minimale obligatoire par feature
1. Impact fonctionnel.
2. Criteres d'acceptation verifies.
3. Strategy de test appliquee.
4. Limites connues et risques restants.

## 9. Repartition equipe et ownership
1. Equipe Audio/Soundboard (toi): owner de US 1.x, 2.x, 3.x, 4.x.
2. Equipe Reseau (autre dev): owner de US 6.x.
3. Zone partagee: US 5.x (UI) et modeles Domain communs.

### 9.1 Regles de dependances inter-equipes
1. Le module Session ne depend pas directement de l'implementation Audio.
2. L'integration Session -> Audio passe uniquement par des interfaces Application.
3. Les changements dans Domain commun et UI demandent validation des deux devs.

### 9.2 Regles de coordination
1. Synchronisation equipe obligatoire minimum 2 fois par semaine.
2. Toute modification cassante d'interface doit etre annoncee avant merge.
3. Les conflits de modeles partages sont resolus en pair review.
