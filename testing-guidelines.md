# Guide des Tests : Vertical Slice Architecture (VSA)

Ce document décrit la stratégie de test pour le projet, axée sur les tests d'intégration rapides utilisant EF Core In-Memory et Alba.

## 1. Principes de Test

- **Priorité aux Tests d'Intégration** : Testez la "tranche" complète (Endpoint -> Wolverine -> DB).
- **Isolation des Tests** : Chaque test doit être indépendant. Utilisez des bases de données In-Memory nommées de manière unique par test ou nettoyez la base après chaque exécution.
- **Vérification du BFF** : Les tests doivent valider que les réponses API correspondent exactement à ce qu'attend le frontend (Contrat BFF).

## 2. Infrastructure de Test

### EF Core In-Memory
Pour les tests, nous configurons le `DbContext` pour utiliser `UseInMemoryDatabase`. C'est idéal pour VSA car :
1. C'est extrêmement rapide.
2. Cela ne nécessite pas de conteneurs Docker ou de bases de données externes.
3. C'est suffisant pour valider la logique des handlers et la persistance de base.

### Alba pour les tests HTTP
Alba permet de faire tourner l'application en mémoire et de l'interroger avec un client HTTP simulé.

## 3. Stratégie d'Isolation avec XUnit Fixtures et Collections

### Principes
Nous utilisons une stratégie d'**isolation intelligente + partage** basée sur les patterns de mutation des tests :

1. **Tests Read-Only** : Partagent une fixture pré-seedée (données communes)
2. **Tests Mutants** : Chacun a sa propre fixture isolée (base de données unique)
3. **Tests Error-Case** : Fixture légère sans seeding

### Fixtures Disponibles

#### CustomersFixture
```csharp
[Collection(nameof(CustomersCollection))]
public class CustomersFeaturesTests
{
    private readonly CustomersFixture _fixture;

    public CustomersFeaturesTests(CustomersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_Register_New_Customer()
    {
        var host = _fixture.Host;
        // Utiliser host pour les tests
    }
}
```

#### DiscogsReadOnlyFixture
Pré-seede les données une fois, partagée entre tous les tests read-only du groupe :
```csharp
[Collection(nameof(DiscogsReadOnlyCollection))]
public class BrowseReleasesTests
{
    private readonly DiscogsReadOnlyFixture _fixture;
    // Les données sont déjà importées via /api/discogs/import/master/1
}
```

#### DiscogsIsolatedFixture
Chaque test obtient sa propre base de données isolée pour les mutations :
```csharp
[Collection(nameof(DiscogsIsolatedCollection))]
public class ImportMasterReleaseTests
{
    private readonly DiscogsIsolatedFixture _fixture;
    // Chaque test a une DB unique
}
```

#### DiscogsErrorCaseFixture
Configuration légère pour les tests d'erreur sans seeding :
```csharp
[Collection(nameof(DiscogsErrorCaseCollection))]
public class QueryReleasesErrorCaseTests
{
    private readonly DiscogsErrorCaseFixture _fixture;
    // Pas de données pré-seedées
}
```

### Utilisation des Collections avec nameof
Toujours utiliser `nameof` pour les références de collection (pas de strings) :

```csharp
// ✅ BON
[Collection(nameof(CustomersCollection))]
[CollectionDefinition(nameof(CustomersCollection))]

// ❌ MAUVAIS
[Collection("Customers Collection")]
[CollectionDefinition("Customers Collection")]
```

Cela permet :
- Validation au compile-time
- Refactoring automatique des noms
- Évite les avertissements "classe inutilisée"

### Exemple Complet : Stratégie par Module

**Customers Module :**
- `CustomersFixture` + `CustomersCollection`
- Tests : `CustomersFeaturesTests`

**Discogs Module :**
- `DiscogsReadOnlyFixture` + `DiscogsReadOnlyCollection` → BrowseReleasesTests, QueryReleasesTests (read-only)
- `DiscogsIsolatedFixture` + `DiscogsIsolatedCollection` → ImportMasterReleaseTests
- `DiscogsErrorCaseFixture` + `DiscogsErrorCaseCollection` → QueryReleasesErrorCaseTests

### Avantages de cette Approche
- **Zéro duplication** : Pas de `builder.ConfigureServices` répété dans chaque test
- **Performance** : Tests read-only partagent les données pré-seedées
- **Sécurité** : Tests mutants isolés, pas d'interférence
- **Maintenabilité** : Stratégie claire et cohérente par type de test

## 3. Structure des Tests

Organisez les tests en suivant la structure des modules et des fonctionnalités :
`PlateformeLocationDisques.Tests/Modules/[ModuleName]/Features/[FeatureName]Tests.cs`

## 4. Skills pour l'Agent

### Écrire un test de tranche (Slice Test)
1. **Initialiser l'hôte Alba** (ou utiliser une classe de base partagée).
2. **Préparer les données** (Seed) directement via le `DbContext`.
3. **Appeler l'Endpoint** via `Alba`.
4. **Vérifier le résultat JSON** (BFF Contract).
5. **Vérifier l'état de la DB** (Side Effects).

### Exemple de Test BFF (Login)
```csharp
[Fact]
public async Task Login_Should_Return_BFF_Token_On_Success()
{
    // Arrange
    // Seeder un utilisateur via DbContext ...

    // Act
    var response = await host.PostJson(new LoginRequest("user@test.com", "pass"), "/api/customers/login");

    // Assert
    response.StatusCodeShouldBe(200);
    var loginResult = response.ReadAsJson<LoginResponse>();
    loginResult.Success.Should().BeTrue();
    loginResult.Token.Should().NotBeNullOrEmpty();
}
```

## 5. Checklist de Validation des Tests
- [ ] Le test utilise-t-il EF Core In-Memory pour la rapidité ?
- [ ] Le test vérifie-t-il le format de réponse du BFF ?
- [ ] Le test couvre-t-il les cas de succès ET les cas d'erreur (ex: 401 Unauthorized) ?
- [ ] Les données de test sont-elles isolées des autres tests ?
