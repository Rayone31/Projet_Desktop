# Dependances Cross-Teams - Audio et Reseau

## 1. Principe
1. Travail parallele autorise, mais integration pilotee par contrats.
2. Aucun couplage direct Infrastructure Audio <-> Infrastructure Reseau.

## 2. Contrats minimaux
1. Session consomme des interfaces Application exposees par Audio.
2. Les identifiants partages (SoundId, SessionId, UserId) restent stables.
3. Les evenements reseau sont traduits en commandes Application.

## 3. Matrice de dependances
1. US 6.1/6.2 dependent d'un modele Soundboard stable (US 1.x base).
2. US 6.3 depend d'un declenchement audio local stable (US 1.x + interfaces).
3. US 6.4/6.5/6.6 dependent du coeur session deja fonctionnel.
4. US 5.x depend de l'integration des streams Audio et Reseau.

## 4. Points de synchronisation
1. Fin Sprint 1: validation contrats Application cibles.
2. Fin Sprint 2: premier test integration creation/rejoindre.
3. Fin Sprint 3: test diffusion son en session.
4. Fin Sprint 4: test persistance + etat session.
5. Fin Sprint 5: fusion complete via branche d'integration.

## 5. Risques techniques
1. Changement d'interface non synchronise.
- Mitigation: annonce pre-merge + review cross-team.
2. Divergence de modeles partages.
- Mitigation: ownership explicite + tests de contrat.
3. Regressions lors de fusion finale.
- Mitigation: integration continue et tests bout en bout.
