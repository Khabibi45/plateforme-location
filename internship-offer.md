# Offre de Stage — Développeur Full Stack (6 mois) | Music Action – Toulouse

## Qui sommes-nous ?

**Music Action** est une boutique toulousaine entièrement dédiée à la production musicale électronique et au DJing — pour les professionnels comme pour les passionnés. Vente, réparation, location d'instruments et de matériel de sonorisation : on vit et on respire la musique.

Dans ce cadre, nous développons **une plateforme web de location de disques vinyles** : un projet concret, utile dès le premier jour, et construit avec une stack moderne orientée production.

---

## Ta mission

Tu rejoins le projet en tant que **développeur full stack**, du premier endpoint au déploiement en production. Ce n'est pas un stage d'observation : tu codes, tu livres, tu déploies.

### Backend (.NET 10)

- Implémenter les modules métier : **Catalog** (catalogue de disques), **Rental** (location) et **Customers** (gestion de compte)
- Suivre l'architecture en place : **Vertical Slice Architecture**, **CQRS avec Wolverine**, **Minimal APIs**
- Écrire les **tests d'intégration** avec Alba et xUnit
- Maintenir la qualité : validation FluentValidation, réponses HATEOAS (`_links`)

### Frontend (Angular 19)

- Développer le **SPA Angular** qui consomme le BFF
- Utiliser les **Signals** Angular pour la gestion d'état
- Respecter la stratégie front définie : navigation par `_links`, DTOs typés, composants Standalone
- Implémenter les écrans : catalogue, recherche, panier, location, compte client

### DevOps & Mise en production

- Conteneurisation **Docker**, déploiement sur serveur **Linux**
- Pipeline **GitHub Actions** : build → tests → lint → déploiement
- Mise en place des **hooks Git** (pre-commit, pre-push)
- Déploiement automatique via **Komodo**

---

## Stack technique

| Couche | Technologie |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, Wolverine |
| Base de données | PostgreSQL 17, EF Core 10 |
| Frontend | Angular 19, Signals, HttpClient |
| Tests | xUnit, Alba, FluentAssertions |
| Infra | Docker, Linux, GitHub Actions, Komodo |

---

## Ce qu'on cherche

- Étudiant(e) en **3e année** d'informatique (licence pro, BUT, bachelor, etc.)
- Tu as déjà fait du **C#** ou du **Java/TypeScript** — tu apprends vite le reste
- Tu es à l'aise avec **Git** et la ligne de commande
- Bonus : tu connais Docker, Angular, ou tu as déjà touché à du CI/CD

Tu n'as pas besoin de tout maîtriser d'avance. Le projet est documenté, l'architecture est posée, et tu seras accompagné(e). Ce qu'on attend avant tout : **de la curiosité et de l'autonomie**.

---

## Agentic Coding : une façon de travailler qu'on prend au sérieux

On ne cherche pas quelqu'un qui copie-colle du code généré par ChatGPT. On cherche quelqu'un qui sait **piloter un agent IA** pour implémenter une feature de bout en bout.

Concrètement, ça veut dire :

- Utiliser **Claude Code** (CLI) ou un éditeur intégré (Cursor, Windsurf…) — pas juste un chat vide
- Donner à l'agent le bon contexte et une tâche claire : *"implémente le endpoint GetReleasesByArtist en suivant les patterns existants"* plutôt que *"écris une fonction qui fait X"*
- Savoir **valider, corriger, et reprendre la main** — ne jamais merger du code qu'on ne comprend pas
- Itérer vite, livrer vite, sans sacrifier la qualité

Tu as déjà expérimenté cette façon de travailler avec les bons outils et des pratiques efficaces ? C'est un vrai plus.

Tu n'y as pas encore touché mais tu es curieux(se) ? On te forme. **La licence Claude est fournie par l'entreprise.**

---

## Conditions

- **Durée** : 6 mois
- **Démarrage** : dès que possible
- **Format** : 100% remote
- **Rémunération** : gratification légale + **bonus selon résultats** (livraisons, qualité du code, mise en prod)

---

## Comment postuler

Envoie-nous :
1. Ton **CV**
2. Un **lien GitHub** ou un exemple de projet (même petit, même scolaire)
3. Deux lignes sur **pourquoi ce projet t'intéresse**

📧 **[adresse email à compléter]**

---

*Music Action – Toulouse | Production musicale, DJing, électronique*
