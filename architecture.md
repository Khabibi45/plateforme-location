# Architecture : Plateforme de Location de Disques (MusicAction)

Ce document décrit l'architecture technique de la plateforme de location de disques, basée sur les principes de **Vertical Slice Architecture (VSA)** et de **Monolithe Modulaire**.

## Principes Directeurs

- **Vertical Slice Architecture (VSA)** : Organisation du code par fonctionnalités (features) plutôt que par couches techniques. Chaque tranche (slice) contient tout ce dont elle a besoin pour fonctionner (API, Logique, Persistance).
- **Monolithe Modulaire** : Séparation logique du système en modules indépendants (ex: Customer, Catalog, Rental) pour maintenir une haute cohésion et un faible couplage, tout en conservant une unité de déploiement simple.
- **CQRS (Command Query Responsibility Segregation)** : Séparation des opérations de lecture (Queries) et d'écriture (Commands).
- **API First & Headless BFF** : L'API est le contrat principal. Le Backend-for-Frontend (BFF) sert de passerelle optimisée pour les clients (ici, un futur client Angular).
- **HATEOAS (Hypermedia as the Engine of Application State)** : Les réponses de l'API incluent des liens hypermedia (`_links`) qui permettent au client de découvrir dynamiquement les actions disponibles. Le frontend n'a pas besoin de connaître la structure des URLs à l'avance, il les découvre via les liens fournis par l'API.

## Pile Technologique

- **Runtime** : .NET 10 (C# 14)
- **Médiateur & Command Bus** : [Wolverine](https://wolverine.netlify.app/) pour le dispatch des messages et le traitement asynchrone.
- **Accès aux Données** : Entity Framework Core 10 (EF Core).
- **Base de Données** :   PostgreSQL  
- **Tests** : xUnit, FluentAssertions, Alba (Tests d'Intégration), EF Core In-Memory.

## Structure de la Solution (.NET 10)

```text
src/
  ├── PlateformeLocationDisques.AppHost/      (Optionnel : .NET Aspire)
  ├── PlateformeLocationDisques.ServiceDefaults/
  ├── PlateformeLocationDisques.WebApi/       (Le Monolithe / BFF)
      ├── Modules/
          ├── Customers/                      (Premier Module)
              ├── Features/                   (Vertical Slices)
                  ├── Register/
                      ├── RegisterCustomerCommand.cs
                      ├── RegisterCustomerHandler.cs
                      ├── RegisterCustomerEndpoint.cs (FastEndpoints ou Minimal API)
                  ├── Login/
                      ├── LoginCommand.cs
                      ├── LoginHandler.cs
                      ├── LoginEndpoint.cs
                  ├── GetAccountInfo/
                      ├── GetAccountInfoQuery.cs
                      ├── GetAccountInfoHandler.cs
              ├── Domain/                     (Entités spécifiques au module)
              ├── Infrastructure/             (DbContext, Repositories spécifiques)
              ├── CustomersModule.cs          (Configuration du module)
          ├── Catalog/                        (Futur module)
          ├── Rental/                         (Futur module)
      ├── Shared/                             (Composants transverses)
          ├── Infrastructure/
          ├── Validation/   
      ├── Data/
      │   ├── AppDbContext.cs
      │   └── SharedEntities/
```

## Flux de Données (CQRS avec Wolverine)

1. **Entrée** : Une requête HTTP arrive sur un Endpoint (Minimal API).
2. **Dispatch** : L'Endpoint envoie une commande ou une requête à Wolverine (`IMessageBus`).
3. **Traitement** : Wolverine trouve le `Handler` correspondant.
   - Les **Commands** modifient l'état via EF Core et peuvent publier des événements.
   - Les **Queries** utilisent EF Core (éventuellement avec `AsNoTracking` ou Dapper) pour retourner des DTOs.
4. **Réponse** : Le résultat est renvoyé au client Angular via le BFF.

## HATEOAS et Hypermedia dans le BFF

Le pattern HATEOAS (Hypermedia as the Engine of Application State) est un principe REST qui permet au client de découvrir dynamiquement les actions disponibles via des liens hypermedia inclus dans les réponses API.

### Principes

- **Découverte dynamique** : Le client n'a pas besoin de connaître à l'avance toutes les URLs de l'API. Il découvre les actions possibles via les liens fournis dans chaque réponse.
- **Couplage faible** : Les URLs peuvent changer côté serveur sans impacter le client, tant que les relations (`rel`) restent cohérentes.
- **BFF optimisé** : Les liens fournis sont adaptés au contexte du client Angular et incluent les paramètres nécessaires (pagination, filtres, etc.).

### Structure des Liens

Chaque réponse DTO inclut un objet `_links` contenant des relations nommées :

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "_links": {
    "self": { "href": "/api/discogs/releases?page=1&pageSize=20" },
    "next": { "href": "/api/discogs/releases?page=2&pageSize=20" },
    "prev": null,
    "byGenre": { "href": "/api/discogs/genres" },
    "byArtist": { "href": "/api/discogs/artists" }
  }
}
```

### Relations Standard

| Relation | Description |
|----------|-------------|
| `self` | Lien vers la ressource actuelle |
| `next` | Page suivante (pagination) |
| `prev` | Page précédente (pagination) |
| `first` | Première page |
| `last` | Dernière page |
| `item` | Lien vers une ressource individuelle |
| `collection` | Lien vers la collection parente |
| `byGenre` | Lien pour filtrer par genre |
| `byArtist` | Lien pour filtrer par artiste |
| `genres` | Liste de tous les genres disponibles |
| `artists` | Liste de tous les artistes disponibles |

### Implémentation

Les liens sont générés dynamiquement dans les Handlers en fonction du contexte de la requête et des données retournées. Chaque Feature peut définir ses propres liens pertinents.

## Module Customers : Gestion de Compte & Connexion

Ce module est le premier à être implémenté. Il gère :
- L'enregistrement des nouveaux clients.
- L'authentification (Login).
- La gestion du profil (Account Management).

### Schéma de données simplifié (Module Customers)
- `Customer` (Id, Email, PasswordHash, Name, MembershipType, JoinDate)

## Authentification et Sécurité

- Utilisation de JWT ou de Cookies sécurisés (BFF Pattern).
- Wolverine middleware pour la gestion des transactions et la validation.


## Technologies

| Technology                   | Purpose                                      |
| ---------------------------- | -------------------------------------------- |
| **.NET 10 Minimal APIs**     | Lightweight HTTP endpoints (LTS release)     |
| **MediatR**                  | Request/response pattern, pipeline behaviors |
| **FluentValidation**         | Declarative validation rules                 |
| **ErrorOr**                  | Result pattern for error handling            |
| **Entity Framework Core 10** | Data access with in-memory or PostgreSQL     |
| **xUnit + FluentAssertions** | Testing framework                            |

## Development Commands

```bash
# Build
dotnet build

# Run (Swagger at http://localhost:5206)
dotnet run --project src/Api/Api.csproj

# Run tests
dotnet test

# Format code
dotnet format
```

##  Data Model and Samples

IDs are generated by code

### no GUID, use ULID

use https://github.com/ByteAether/Ulid



In development mode, the API automatically seeds sample Customers and Records

### Customers

| ID                           | Name        | Email                       |
|------------------------------| ----------- | --------------------------- |
| `01BX5ZZKBKACTAV9WEVGEMMVRY` | John Smith  | `john.smith@example.com`    |
| `01BX5ZZKBKACTAV9WEVGEMMVRZ` | Jane Doe    | `jane.doe@example.com`      |
| `01BX5ZZKBKACTAV9WEVGEMMVR0` | Bob Johnson | `bob.johnson@example.com`   |

### Records
   

### Example: Book the rental of a record release

basically it goes like:

/api/rental/customer/{customerId}/askToRent/{recordId}

the logic will compute id the selected record is available and put it inot the basket of the customer



```bash
to be done
```

## Database

**Default:** In-memory database (no setup required)

### Docker Compose (Recommended for PostgreSQL)

One command to start PostgreSQL:

```bash
# Start PostgreSQL container
docker compose up -d

# Apply migrations (first time only)
UseInMemoryDatabase=false dotnet ef database update --project src/Application --startup-project src/Api

# Run API with PostgreSQL
dotnet run --project src/Api --launch-profile Docker
```

**Cleanup:**

```bash
# Stop container (data preserved)
docker compose down

# Stop and delete all data
docker compose down -v
```

### Manual PostgreSQL Setup

If you prefer not to use docker-compose, update `src/Api/appsettings.json`:

```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=VerticalSliceDb;Username=postgres;Password=yourPassword"
  }
}
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add "MigrationName" --project src/Application --startup-project src/Api --output-dir Infrastructure/Persistence/Migrations

# Apply migrations
dotnet ef database update --project src/Application --startup-project src/Api
