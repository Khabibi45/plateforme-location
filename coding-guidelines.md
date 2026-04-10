# Agentic Coding Guidelines : Plateforme de Location de Disques (MusicAction)

Ce document fournit des directives pour le développement de l'application en suivant les principes de **Vertical Slice Architecture (VSA)**, de **Monolithe Modulaire** et de **CQRS avec Wolverine**.

## 1. Principes d'Architecture

- **Vertical Slice Architecture (VSA)** : Organisez le code par fonctionnalité (Feature), pas par couche technique. Chaque dossier de fonctionnalité contient tout ce dont il a besoin pour fonctionner.
- **Monolithe Modulaire** : Séparez les domaines d'activité en modules (ex: `Customers`, `Catalog`, `Rental`). La communication entre modules doit être asynchrone (via messages) ou via des contrats bien définis.
- **CQRS avec Wolverine** : Utilisez Wolverine pour le dispatching des commandes et des requêtes. Wolverine remplace avantageusement MediatR dans ce projet.

## 2. Structure d'une fonctionnalité (Slice)

Chaque fonctionnalité doit être contenue dans son propre dossier sous `Modules/[ModuleName]/Features/[FeatureName]/`.

Une tranche typique comprend :
- **Message (Command/Query)** : Un `record` immuable représentant l'intention.
- **Handler** : Une classe ou une méthode statique qui traite le message.
- **Endpoint** : Un point d'entrée Minimal API qui délègue immédiatement au `IMessageBus` de Wolverine.
- **DTOs** : Objets de transfert de données spécifiques à la fonctionnalité.

### Exemple de structure de fichier :
```csharp
namespace PlateformeLocationDisques.WebApi.Modules.Customers.Features.Register;

// 1. Le Message (Contrat)
public record RegisterCustomer(string Email, string Password, string Name);

// 2. Le Handler (Logique métier)
public static class RegisterCustomerHandler
{
    // Wolverine découvre automatiquement les méthodes Handle/Consume
    public static async Task<CustomerRegistered> Handle(
        RegisterCustomer command, 
        CustomersDbContext dbContext)
    {
        // ... Logique ici ...
        return new CustomerRegistered(Guid.NewGuid());
    }
}

// 3. L'Endpoint (Point d'entrée)
public static class RegisterCustomerEndpoint
{
    public static void MapRegisterCustomer(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/customers/register", async (RegisterCustomer command, IMessageBus bus) =>
        {
            // AUCUNE LOGIQUE ICI. Juste le dispatch.
            var result = await bus.InvokeAsync<CustomerRegistered>(command);
            return Results.Ok(result);
        })
        .WithTags("Customers");
    }
}
```x

## 3. Directives de Développement (Best Practices)

### BFF Endpoints (Backend-for-Frontend) and Adapters
- Les endpoints doivent  un pattern Result  nécessaires au frontend (Angular).
- Le front end a besoin  de connaitre les raisons en cas d'echec de traitement dans le endpoint (et potentiellement les adapteurs), ne pas retourner null
- pas de try/catch, il sera géré par la web application
- Utilisez des DTOs de réponse spécifiques pour chaque endpoint (Data Shaping).
- ces règles s'appliquent pour les Adapters aussi
- Ne retournez JAMAIS les entités de domaine directement.
- La sécurité (JWT, Cookies) doit être gérée au niveau du BFF.

### Pas de fuite de logique dans les Endpoints (No Logic Leaking)
- Les endpoints Minimal API ne doivent contenir **aucune** logique métier, validation complexe ou accès direct à la base de données.
- Leur seul rôle est de transformer la requête HTTP en un message (Command/Query) et de l'envoyer au bus Wolverine.

### Pas de "God" Slices
- Une tranche verticale doit être focalisée. Si une fonctionnalité devient trop complexe, divisez-la en tranches plus petites ou utilisez des sous-domaines.
- Évitez les services "fourre-tout" partagés. Si deux tranches ont besoin de la même logique, envisagez de la dupliquer légèrement (DRY n'est pas absolu en VSA) ou de créer un composant d'infrastructure partagé très spécifique.

### Fonctionnalités transverses via Wolverine Middleware (No Inheritance)
- N'utilisez **pas de classes de base** ou d'héritage pour les handlers ou les commandes.
- Utilisez les **Wolverine Middleware** (Policies/Behaviors) pour gérer les aspects transverses :
    - Validation (FluentValidation).
    - Journalisation (Logging).
    - Transactions (Outbox pattern).
    - Gestion des erreurs.

### Persistance et Isolation
- Chaque module possède son propre `DbContext` et son propre schéma de base de données (ex: `customers`, `catalog`).
- Les jointures entre tables de modules différents sont proscrites au niveau SQL. La corrélation se fait par ID au niveau applicatif ou via des événements asynchrones.

## 4. Validation

Utilisez **FluentValidation**. Wolverine peut être configuré pour exécuter automatiquement les validateurs avant le handler.

```csharp
public class RegisterCustomerValidator : AbstractValidator<RegisterCustomer>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

## 5. Tests

- **Tests d'Intégration (BFF style)** : Étant donné que nous utilisons VSA, les tests qui traversent toute la tranche (de l'API à la DB) sont les plus précieux.
- **EF Core In-Memory** : Pour les tests de développement et d'intégration rapide, utilisez `UseInMemoryDatabase`. Cela permet de tester les interactions avec le `DbContext` sans infrastructure complexe.
- **Alba** : Utilisez `Alba` pour simuler des appels HTTP et vérifier les réponses JSON.
- **Wolverine Integration** : Testez que les messages sont correctement dispatchés et que les effets de bord (ex: Outbox) sont respectés.

Voir le fichier `testing-guidelines.md` pour plus de détails.

## 6. Checklist pour l'Agent

Avant de soumettre une nouvelle fonctionnalité, vérifiez :
1. [ ] La fonctionnalité est-elle dans son propre dossier `Features` ?
2. [ ] L'endpoint est-il une simple passerelle vers `IMessageBus` ?
3. [ ] Le handler est-il indépendant (pas d'héritage de base) ?
4. [ ] La validation est-elle gérée par un middleware ou un validateur séparé ?
5. [ ] Le `DbContext` utilisé est-il celui du module concerné ?
6. [ ] Les meilleures pratiques EF Core sont-elles respectées (voir `efcore-cqrs-best-practices.md`) ?
