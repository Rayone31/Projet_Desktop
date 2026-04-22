# Strategie Git - Travail en parallele

## 1. Branches du projet
1. master: branche principale de developpement et d'integration.
2. audio: branche de travail pour le stream Audio/Soundboard.
3. reseau: branche de travail pour le stream Reseau/Session.

## 3. Regles de Pull Request
1. Toute fusion passe par PR (pas de push direct sur master).
2. Rebase obligatoire sur master avant PR.
3. CI verte obligatoire.
4. Au moins 1 review validee.
5. Changement sur zone partagee: approbation des deux devs.

## 4. Cadence de synchronisation
1. Deux points de merge vers master par semaine minimum.
2. Point de synchronisation technique minimum deux fois/semaine.
3. Si conflit bloquant > 1h: ouvrir une tache de resolution cross-team.

## 5. Nommage commits
1. feat(audio): ...
2. feat(session): ...
3. fix(audio): ...
4. fix(session): ...
5. docs(...): ...
6. test(...): ...

## 6. Politique de conflit
1. Conflit dans Audio module: arbitrage Dev Audio.
2. Conflit dans Session module: arbitrage Dev Reseau.
3. Conflit dans Domain/UI partage: resolution commune obligatoire.

## 7. Preconditions merge release
1. Stories Must vertes.
2. Tests cross-team verts.
3. Documentation a jour.
