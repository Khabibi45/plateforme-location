# Questionnaire Technique — Stage Développeur Full Stack

> Ce questionnaire évalue ta capacité à lire et comprendre une base de code existante.
> Il n'y a pas de piège : prends le temps d'explorer le dépôt avant de répondre.
> Les réponses courtes et précises sont préférées aux réponses longues et vagues.

---

### Note personnelle avant de commencer

J'ai eu des cours de C en école, mais au quotidien je travaille surtout en PHP et JS, beaucoup avec l'aide de Claude. Découvrir un projet en C# / .NET avec Wolverine, EF Core et CQRS, c'est un exercice vraiment nouveau pour moi.

J'ai utilisé Claude Code pour comprendre l'architecture et formuler une première version des réponses, puis je les ai reprises avec mes mots. Les Q14, Q15 et Q16 sont restées telles quelles (générées par l'IA) — c'est indiqué.

---

## Partie 1 — Architecture générale (niveau débutant)

**Q1.** Le code backend est organisé en dossiers `Features/` à l'intérieur de chaque module, plutôt qu'en dossiers `Controllers/`, `Services/`, `Repositories/` comme on le voit souvent dans les tutoriels.

Pourquoi ce choix ? 

```
C'est ce qu'on appelle la Vertical Slice Architecture. Au lieu de ranger par type de fichier
(tous les controllers ensemble, tous les models ensemble...), on range par fonctionnalité.
Ça m'a un peu déstabilisé au début parce que j'ai toujours fait du MVC classique.
```

Quel est l'avantage concret pour un développeur qui travaille sur une seule fonctionnalité ?

```
Tout est au même endroit. Si je bosse sur GetReleasesByGenre, j'ouvre le dossier et j'ai
les 3 fichiers (le record, le handler, l'endpoint). Pas besoin de naviguer entre 4 dossiers.

Et si on supprime la feature, on supprime juste le dossier.
```

---

**Q2.** Le projet contient deux modules distincts : `Customers` et `DiscogsImportation`. Chacun a son propre `DbContext`.

Pourquoi ne pas utiliser un seul `DbContext` partagé pour toute l'application ?

```
C'est le principe du monolithe modulaire. Chaque module gère sa propre partie de la base
avec son propre schéma PostgreSQL ("customers" et "discogs"). Comme ça un bug coté Discogs
peut pas toucher les données des clients, et si un jour on veux extraire un module en
microservice c'est déjà découplé.

En PHP natif j'avais une seule connexion PDO globale pour tout, c'est l'approche inverse.
Pour un projet de cette taille je trouve ça un peu lourd, mais je comprend la logique sur
du long terme.
```

---

**Q3.** Dans le module `DiscogsImportation`, il existe une interface `IDiscogsClient` avec deux implémentations : `FakeDiscogsClient` et `DiscogsApiClient`.

a) Où est décidé laquelle des deux est utilisée à l'exécution ?

```
Dans Program.cs, y'a une condition sur l'environnement. En Development → FakeDiscogsClient
(données en mémoire), sinon → DiscogsApiClient (vraie API). Le handler demande juste un
IDiscogsClient sans savoir lequel il reçoit.
```


b) Pourquoi ce mécanisme plutôt que d'appeler directement l'API Discogs partout dans le code ?

```
Pour le dev pas besoin de token ni d'internet, et pour les tests pas de requête réseau.
Si Discogs change son API on touche qu'à un seul fichier, le reste du code parle à
l'interface donc il est pas impacté.

C'est le pattern Adapter avec inversion de dépendance (le D de SOLID).
```
---

## Partie 2 — Flux d'une requête (niveau intermédiaire)

**Q4.** Trace le chemin complet d'une requête HTTP `POST /api/discogs/import/master/1234` :

- Quel fichier reçoit la requête en premier ?
```
ImportMasterReleaseEndpoint.cs, c'est là qu'est déclarée la route POST. Elle est branchée
au pipeline depuis Program.cs avec app.MapImportMasterRelease().
```
- Que se passe-t-il ensuite (sans décrire le code ligne par ligne — décris les étapes logiques) ?
```
L'endpoint récupère le discogsId, crée un objet ImportMasterRelease(1234) et l'envoie à
Wolverine. Wolverine trouve tout seul le bon handler (celui dont la méthode Handle accepte
un ImportMasterRelease).

Le handler vérifie d'abord si le master existe déjà en base (anti-doublon). Sinon il appelle
le client Discogs, vérifie le résultat, transforme les données en entité du domaine et
sauvegarde. Le résultat remonte au client en JSON.
```
- Quel objet est retourné au client ? Comment le client peut-il utiliser cet objet ?
```
Un record MasterReleaseImported avec l'Id ULID interne, le DiscogsId, le Title et un booléen
AlreadyExisted. Le client utilise l'Id pour faire un GET /api/discogs/master-releases/{Id}
ensuite.
```

---

**Q5.** Dans les handlers Wolverine, les dépendances (`IDiscogsClient`, `DiscogsDbContext`, etc.) sont passées en **paramètres de méthode**, pas via un constructeur.

Est-ce que tu reconnaîs ce mécanisme ? Comment s'appelle-t-il et pourquoi est intéressant ici ?
```
C'est du Method Injection. La classe handler est statique, donc pas de constructeur, et les
dépendances arrivent directement dans la méthode Handle().

Du coup y'a aucun état partagé entre les requetes, chaque appel reçoit ses propres instances.
Et pour tester c'est pratique parce qu'on passe les mocks directement en paramètre.
```

---

**Q6.** Que fait concrètement cette ligne dans `Program.cs` :

```csharp
opts.UseEntityFrameworkCoreTransactions();
```

Quel problème cela résout-il sans que le développeur ait à y penser ?

```
Ça active les transactions automatiques. Si le handler réussit → commit, si ça plante →
rollback. Le développeur n'écrit aucun try/catch ni beginTransaction.

En PHP natif avec PDO je devais faire ça manuellement à chaque opération sensible :
$pdo->beginTransaction(), try, $pdo->commit(), catch, $pdo->rollBack(). Ici c'est
transparent.
```

---

## Partie 3 — Données et persistance (niveau intermédiaire)

**Q7.** Les identifiants des entités (ex: `MasterRelease.Id`) sont de type `Ulid` et non `Guid`.

a) Quelle est la différence principale entre un ULID et un GUID ?

```
Un GUID c'est complètement aléatoire, deux à la suite ont aucun ordre. Un ULID c'est un
timestamp + une partie aléatoire, donc les premiers caractères encodent la date de création
et ils se trient chronologiquement tout seul.
```

b) Pourquoi ce choix peut être intéressant pour une base de données ?

```
Les index B-tree sont optimisés pour les insertions séquentielles. Avec des GUIDs aléatoires
chaque insertion va n'importe où dans l'index → fragmentation. Avec des ULIDs ça atterrit
toujours à la fin → c'est plus performant.

Et trier par Id revient à trier par date de création, ça évite une colonne created_at en plus.
Honnêtement c'est un truc auquel j'avais jamais réfléchi avant, j'utilisais des auto-increment
sans me poser la question.
```

---

**Q8.** Dans la configuration EF Core du module `DiscogsImportation`, on voit :

```csharp
masterRelease.Property(m => m.Genres)
    .HasColumnType("jsonb");
```

Qu'est-ce que `jsonb` dans PostgreSQL ?
```
C'est un type de colonne qui stocke du JSON en binaire pré-parsé. Genres c'est une List<string>
en C#, et en base ça donne ["Rock", "Progressive Rock"] dans une seule colonne. Contrairement
au type JSON classique, jsonb est indexable et on peut faire des requêtes dessus.
```


Pourquoi stocker les genres ainsi plutôt que dans une table séparée ?
```
Parce que c'est juste une liste de chaînes, pas une entité avec sa propre logique. Pas besoin
d'une table pivot ni de JOIN pour ça.

J'aurais probablement fait une table genres + table de liaison par réflexe, mais ici ça n'a
aucun sens : les genres n'ont pas de cycle de vie propre, ils existent que pour qualifier
un album.
```

---

**Q9.** Dans la configuration EF Core, certaines propriétés sont mappées avec `OwnsMany()` :

```csharp
masterRelease.OwnsMany(m => m.Tracklist, track => { ... });
```

Qu'est-ce qu'un "Owned Type" en EF Core ? 
```
C'est une entité qui appartient à une autre et qui peut pas exister toute seule. Un Track
existe pas sans son MasterRelease, on cherche jamais "le track 42" tout seul, c'est toujours
les tracks d'un album donné.

Y'en a plusieurs dans DiscogsDbContext (Artists, Tracklist, Images, Videos en OwnsMany,
Community en OwnsOne).
```


Quel est l'avantage par rapport à une entité indépendante avec sa propre table ?
```
Y'a pas de DbSet séparé : on accède aux tracks que via leur parent. Le chargement et la
suppression en cascade sont automatiques. Et surtout, le modèle empêche directement de créer
un Track orphelin — c'est la regle métier qui est encodée dans la structure.
```

---

## Partie 4 — Tests (niveau intermédiaire)

**Q10.** Le projet définit plusieurs fixtures de test : `DiscogsReadOnlyFixture`, `DiscogsIsolatedFixture`, `DiscogsErrorCaseFixture`.

Pourquoi avoir trois fixtures différentes plutôt qu'une seule ? 

```
Pour que les tests se marchent pas dessus. Chaque fixture crée sa propre base en mémoire
avec un nom unique. La ReadOnlyFixture pré-remplit avec des données, les deux autres partent
d'une base vide.

Avec une seule fixture, l'ordre d'exécution des tests pourrait changer leurs résultats —
des tests "flaky".
```

Explique dans quel cas tu utiliserais chacune.

```
ReadOnlyFixture pour les tests de lecture (recherche, filtres, liens HATEOAS).
IsolatedFixture pour les tests d'écriture (import, doublons).
ErrorCaseFixture pour les cas d'erreur (404, formats invalides), séparée pour pas interférer.
```
---

**Q11.** Les collections XUnit sont définies avec `nameof()` :

```csharp
[CollectionDefinition(nameof(CustomersCollection))]
public class CustomersCollection : ICollectionFixture<CustomersFixture> { }
```

Pourquoi utiliser `nameof()` ici plutôt qu'une chaîne de caractères en dur comme `"CustomersCollection"` ?
```
nameof() retourne le nom de la classe en string à la compilation. Si on renomme la classe,
ça suit partout automatiquement. Avec une string en dur, le lien casse silencieusement et
on s'en rend compte qu'au runtime, ce qui est typiquement le genre d'erreur vicieuse.
```

---

**Q12.** Dans les tests, on utilise **Alba** pour appeler les endpoints HTTP plutôt qu'un `HttpClient` classique.

Quelle est la différence fondamentale ?
```
Alba lance toute l'application ASP.NET en mémoire, sans vrai serveur HTTP. Ça démarre le
pipeline complet (routing, middleware, DI) mais tout est in-process.
```

Pourquoi Alba est-il plus adapté aux tests d'intégration dans ce contexte ?
```
C'est rapide vu qu'y'a pas de réseau, et la syntaxe est lisible (_.Post.Url(...) +
_.StatusCodeShouldBeOk()). Alba est aussi développé par la même équipe que Wolverine,
donc l'intégration entre les deux est native.
```

---

## Partie 5 — Gestion des erreurs (niveau avancé)

**Q13.** Le handler `ImportMasterRelease` utilise un objet `DiscogsResult<T>` pour représenter le résultat de l'appel à l'API Discogs.

a) Quels sont les cas d'erreur possibles (regarde le code) ?
```
Dans DiscogsResult.cs y'a un enum avec 4 cas : NotFound (la ressource existe pas),
ApiError (Discogs renvoie un code d'erreur HTTP), NetworkError (timeout, DNS...) et
DeserializationError (le JSON peut pas être parsé).
```
b) Quelle est la différence entre cette approche et lever une exception directement ?
```
Avec une exception, la signature de la méthode dit pas qu'une erreur peut arriver. En PHP
natif on lance des throw new \Exception un peu partout et faut deviner quoi catcher.

Ici le type de retour dit explicitement que ça peut échouer, et on est obligé de vérifier
IsSuccess avant d'utiliser la valeur. C'est le "Result Pattern", un truc qu'on voit en
C#/Rust/Go mais pas vraiment en PHP natif.
```
c) Quel avantage concret pour le code appelant ?
```
Le handler récupère le résultat, vérifie result.IsSuccess avec un simple if et continue.
Pas de try/catch, et on peut réagir différemment selon le ErrorKind si besoin. C'est plus
lisible une fois qu'on s'y habitue.
```
---

**Q14.** *(réponse générée par IA — Claude Code)*

Dans ce projet, les handlers ne contiennent **aucun `try/catch`**. Pourtant des erreurs peuvent survenir (réseau, base de données, etc.).

Comment sont-elles gérées ? Qui s'en occupe  ou devrait s'en occuper?
```
J'ai vérifié : aucun try/catch dans ImportMasterReleaseHandler.cs,
GetReleasesByGenreHandler.cs, GetReleasesByArtistHandler.cs — aucun handler n'en a.

Deux mécanismes se complètent :

1. Wolverine (le middleware) — Program.cs lignes 66-70 :
   builder.Host.UseWolverine(opts => { opts.UseEntityFrameworkCoreTransactions(); });
   
   Wolverine orchestre l'appel aux handlers. Si une exception non catchée remonte,
   il peut appliquer des politiques de retry, dead-letter queue, ou logging —
   configurables globalement. Les erreurs sont gérées à UN endroit, pas dans chaque handler.

2. UseEntityFrameworkCoreTransactions() (même ligne 69) :
   Si une exception survient pendant un handler qui modifie la base,
   la transaction est automatiquement rollback. Pas de données corrompues.

3. Le pipeline ASP.NET Core (devrait s'en occuper) :
   En production, un middleware global (UseExceptionHandler ou ProblemDetails)
   devrait intercepter les exceptions et retourner une réponse HTTP propre (500)
   au lieu de laisser le serveur crasher.

Philosophie : les handlers font la logique MÉTIER. L'infrastructure
(transactions, retry, error handling HTTP) est gérée par les couches supérieures.
C'est la séparation des responsabilités appliquée à la gestion d'erreurs.
```

---

## Partie 6 — HATEOAS et design API (niveau avancé)

**Q15.** *(réponse générée par IA — Claude Code)*

Les réponses de l'API contiennent systématiquement un objet `_links`. Par exemple :

```json
{
  "id": "...",
  "title": "The Dark Side of the Moon",
  "_links": {
    "self": "/api/discogs/masters/...",
    "releases": "/api/discogs/releases?masterId=..."
  }
}
```

a) Comment s'appelle ce principe de design d'API ?
```
HATEOAS — Hypermedia As The Engine Of Application State.
(Niveau 3 du Richardson Maturity Model pour les APIs REST)

PREUVE — le code qui construit ces liens :

LinkBuilder.cs lignes 13-52 : CreatePaginationLinks() génère self, first, last, prev, next
Links.cs ligne 5 : la classe hérite de Dictionary<string, Link?> — c'est un dictionnaire de liens
Link.cs ligne 5 : record Link(string Href, string Method = "GET", string? Type = null)

Et les extensions pour la navigation :
  LinkBuilder.cs ligne 83 : AddBrowseLinks() → ajoute genres, artists, releases, masterReleases
  LinkBuilder.cs ligne 94 : AddGenreLink(genre) → lien vers /api/discogs/releases/genre/{genre}
  LinkBuilder.cs ligne 108 : AddArtistLink(artist) → lien vers /api/discogs/releases/artist/{artist}

Chaque réponse contient des liens qui disent au client "voici ce que tu peux faire ensuite".
C'est le web (liens hypertexte) appliqué aux APIs.
```

b) Quel est l'avantage pour le frontend Angular qui consomme cette API ?
```
1. Découplage des URLs — le frontend ne code pas les URLs en dur.
   Si l'API change ses routes (ex: /api/v2/releases), le frontend suit
   les _links automatiquement — pas de modification côté Angular.

2. Navigation dynamique — après un GET /api/discogs/genres, chaque genre
   dans la réponse contient un lien "byGenre" (LinkBuilder.cs ligne 102)
   qui dit exactement comment récupérer les releases de ce genre.

3. Pagination sans logique client — les liens prev/next/first/last sont
   calculés par l'API (LinkBuilder.cs lignes 32-49). Le frontend n'a qu'à
   les suivre, sans calculer les offsets lui-même.

4. Évolutivité — si l'API ajoute une action (ex: "addToCart"), elle apparaît
   dans _links. Le frontend peut la proposer sans mise à jour de son code.

5. Un seul point d'entrée — le client connaît l'URL racine (/), puis
   découvre tout via les liens. Comme naviguer sur le web en cliquant.
```

---

## Partie 7 — Feature à implémenter (pratique)

> Cette dernière partie est intentionnellement ouverte. Il n'y a pas une seule bonne réponse.
> On évalue ta façon de raisonner, pas la perfection du code.

---

**Q16.** *(réponse générée par IA — Claude Code)*

Le module `DiscogsImportation` permet déjà de chercher des releases par genre (`GetReleasesByGenre`) ou par artiste (`GetReleasesByArtist`).

**Implémente ou pseudo-code une nouvelle feature : `GetReleasesByLabel`** — retourner la liste des releases associées à un label donné (ex : "Blue Note Records"), avec pagination.

Tu peux t'inspirer librement des features existantes. On attend :

1. La définition du message (record `GetReleasesByLabel`)
2. La signature du handler et sa logique principale (en pseudo-code ou vrai C# — au choix)
3. La définition de l'endpoint (route, méthode HTTP, paramètres)
4. Les `_links` que tu inclurais dans la réponse

**Bonus** : Que faudrait-il vérifier ou tester en priorité pour valider cette feature ?

```
// ============================================================
// RAISONNEMENT — Comment j'ai construit cette réponse
// ============================================================
//
// 1. J'ai regardé GetReleasesByGenre et GetReleasesByArtist : même structure 3 fichiers
//
// 2. GetReleasesByGenre filtre sur une List<string> :
//    GetReleasesByGenreHandler.cs ligne 19 : .Where(r => r.Genres.Contains(query.Genre))
//
// 3. GetReleasesByArtist filtre sur une collection Owned Type :
//    GetReleasesByArtistHandler.cs lignes 20-23 :
//      .Where(r => r.Artists.Any(a => a.Name.Contains(artistName) || ...))
//
// 4. Les Labels sont AUSSI des Owned Types :
//    DiscogsDbContext.cs lignes 199-207 : release.OwnsMany(r => r.Labels, label => { ... })
//    ReleaseLabel.cs lignes 10-12 : propriétés Name et CatalogNumber
//
// 5. Donc le filtre sera comme GetReleasesByArtist (Any sur collection owned),
//    mais sur Labels au lieu d'Artists
//
// 6. J'ajoute CatalogNumber dans le DTO car c'est spécifique aux labels
//    (ReleaseLabel.cs ligne 17 : public string CatalogNumber)
//    et c'est une info cruciale pour les collectionneurs (ex: "SHVL 804")

// ============================================================
// FICHIER 1 : GetReleasesByLabel.cs (les messages)
// Inspiré de : GetReleasesByGenre.cs (même structure de records)
// ============================================================

public record GetReleasesByLabel(
    string LabelName,
    int Page = 1,
    int PageSize = 20
);

public record GetReleasesByLabelResult(
    ReleaseByLabelItemDto[] Items,
    string LabelName,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record ReleaseByLabelItemDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string? Country,
    string[] Genres,
    string[] Artists,
    string? CatalogNumber,   // ← spécifique aux labels, absent de GetReleasesByGenre
    string? Thumb,
    string? Format
);

// ============================================================
// FICHIER 2 : GetReleasesByLabelHandler.cs (la logique)
// Inspiré de : GetReleasesByArtistHandler.cs (filtre sur Owned Type)
// ============================================================

public static class GetReleasesByLabelHandler
{
    public static async Task<GetReleasesByLabelResult> Handle(
        GetReleasesByLabel query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var labelName = query.LabelName;

        // Filtre sur la collection Labels (Owned Type)
        // Même logique que GetReleasesByArtistHandler ligne 20 :
        //   .Where(r => r.Artists.Any(a => a.Name.Contains(artistName)))
        // Mais adapté pour Labels :
        var filteredQuery = dbContext.Releases
            .AsNoTracking()
            .Where(r => r.Labels.Any(l => l.Name.Contains(labelName)));

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var items = await filteredQuery
            .OrderByDescending(r => r.Year)
            .ThenBy(r => r.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReleaseByLabelItemDto(
                Id: r.Id.ToString(),
                DiscogsId: r.DiscogsId,
                Title: r.Title,
                Year: r.Year,
                Country: r.Country,
                Genres: r.Genres.ToArray(),
                Artists: r.Artists.Select(a => a.Name).ToArray(),
                CatalogNumber: r.Labels
                    .Where(l => l.Name.Contains(labelName))
                    .Select(l => l.CatalogNumber)
                    .FirstOrDefault(),
                Thumb: r.Thumb,
                Format: r.Formats.FirstOrDefault() != null
                    ? r.Formats.OrderBy(f => f.Id).First().Name
                    : null
            ))
            .ToArrayAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new GetReleasesByLabelResult(items, query.LabelName,
            totalCount, query.Page, query.PageSize, totalPages);
    }
}

// ============================================================
// FICHIER 3 : GetReleasesByLabelEndpoint.cs (la route)
// Inspiré de : GetReleasesByGenreEndpoint.cs (même structure)
// ============================================================

// Route : GET /api/discogs/releases/label/{labelName}?page=1&pageSize=20
// Méthode : GET (lecture seule)
// Paramètres : labelName (path), page et pageSize (query string, optionnels)

public static class GetReleasesByLabelEndpoint
{
    public static void MapGetReleasesByLabel(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases/label/{labelName}",
            async (string labelName, int? page, int? pageSize,
                   IMessageBus bus, CancellationToken ct) =>
            {
                var query = new GetReleasesByLabel(
                    LabelName: labelName,
                    Page: page ?? 1,
                    PageSize: pageSize is > 0 and <= 100 ? pageSize.Value : 20
                );
                var result = await bus.InvokeAsync<GetReleasesByLabelResult>(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetReleasesByLabel")
            .WithTags("Discogs Importation")
            .Produces<GetReleasesByLabelResult>(200);
    }
}

// + Enregistrer dans Program.cs : app.MapGetReleasesByLabel();

// ============================================================
// _links HATEOAS — inspiré de LinkBuilder.cs lignes 83-117
// ============================================================
// {
//   "_links": {
//     "self":   "/api/discogs/releases/label/Harvest?page=1&pageSize=20",
//     "first":  "/api/discogs/releases/label/Harvest?page=1&pageSize=20",
//     "last":   "/api/discogs/releases/label/Harvest?page=3&pageSize=20",
//     "prev":   null,
//     "next":   "/api/discogs/releases/label/Harvest?page=2&pageSize=20",
//     "genres": "/api/discogs/genres",
//     "artists":"/api/discogs/artists"
//   }
// }
// On réutilise LinkBuilder.CreatePaginationLinks() pour self/first/last/prev/next
// et on ajoute un AddLabelLink() dans LinkBuilder par cohérence.

// ============================================================
// BONUS — Tests prioritaires
// ============================================================
// 1. Nominal : importer la release 1000 (FakeDiscogsClient ligne 143 : Label "Harvest")
//    puis GET /api/discogs/releases/label/Harvest → vérifier qu'on la retrouve
// 2. Pagination : plusieurs releases du même label → vérifier page/totalPages
// 3. Recherche partielle : "Harv" doit matcher "Harvest" (Contains)
// 4. Label inexistant : GET /releases/label/Inconnu → 200 avec Items vide, pas 404
// 5. CatalogNumber : vérifier que "SHVL 804" est retourné (FakeDiscogsClient ligne 146)
// 6. Casse : "harvest" vs "Harvest" → tester le comportement
```

---

*Bonne chance — et n'hésite pas à expliquer ton raisonnement même quand tu n'es pas sûr(e) de la réponse.*
