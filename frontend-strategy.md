# Stratégie Frontend : Angular & BFF

Ce document décrit l'architecture et les conventions du frontend Angular, ainsi que son intégration avec le Backend-for-Frontend (BFF) .NET. Il est destiné à tout développeur rejoignant le projet.

---

## Vue d'ensemble

```
┌─────────────────────────────────────────────────────┐
│              Navigateur (Client)                    │
│  ┌──────────────────────────────────────────────┐  │
│  │          Angular SPA                         │  │
│  │  features/ ── core/ ── shared/               │  │
│  └──────────────────┬───────────────────────────┘  │
└─────────────────────┼───────────────────────────────┘
                      │ HTTP/HTTPS (JSON + _links)
                      ▼
┌─────────────────────────────────────────────────────┐
│        BFF – PlateformeLocationDisques.WebApi        │
│  Endpoints Minimal API  →  Wolverine Bus             │
│  DTOs optimisés Angular  +  HATEOAS (_links)         │
└─────────────────────────────────────────────────────┘
                      │
                      ▼
              PostgreSQL 17
```

**Règle fondamentale** : Angular ne parle qu'au BFF. Jamais à PostgreSQL, jamais à Discogs directement. Le BFF est le seul point d'entrée.

---

## Pourquoi un BFF ?

Le pattern **Backend-for-Frontend** permet de :

- Retourner exactement les données dont Angular a besoin (pas plus, pas moins)
- Centraliser la sécurité (JWT / cookies) côté serveur
- Masquer la complexité interne du backend (CQRS, modules, etc.)
- Adapter le format des réponses au client sans toucher au domaine métier

> **Pour le développeur junior** : tu n'as pas à te préoccuper du fonctionnement interne du backend (Wolverine, EF Core, etc.). Tu travailles uniquement avec les endpoints documentés dans Scalar (`https://localhost:5001/scalar/v1`).

---

## Stack Frontend

| Technologie | Rôle |
|---|---|
| **Angular 19+** | Framework SPA (Standalone Components) |
| **Angular HttpClient** | Appels HTTP vers le BFF |
| **Angular Signals** | Gestion d'état réactive (préféré à NgRx pour ce projet) |
| **Angular Router** | Navigation côté client |
| **Angular Forms** (Reactive) | Formulaires (inscription, connexion, etc.) |

---

## Structure du projet Angular

```
src/
  app/
    core/                     # Services singleton (injectés à la racine)
      auth/
        auth.service.ts       # Gestion session / JWT
      http/
        api.service.ts        # Wrapper HttpClient de base
        hateoas.service.ts    # Résolution des _links
      interceptors/
        auth.interceptor.ts   # Ajout automatique du token
        error.interceptor.ts  # Gestion globale des erreurs HTTP
    shared/                   # Composants/pipes/directives réutilisables
      components/
        pagination/
        loading-spinner/
      pipes/
    features/                 # Miroir des modules backend
      customers/
        register/
        login/
        account/
      catalog/                # (à venir)
      rental/                 # (à venir)
    layout/
      shell/                  # Composant racine (nav, footer)
  environments/
    environment.ts            # URL du BFF en dev
    environment.prod.ts       # URL du BFF en prod
```

> **Convention** : chaque dossier dans `features/` correspond à un module du BFF. Les noms sont identiques pour faciliter la navigation entre le code front et back.

---

## Communication avec le BFF

### Configuration de l'URL de base

```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:5001'
};
```

### Service HTTP de base

```typescript
// core/http/api.service.ts
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  get<T>(url: string): Observable<T> {
    // url peut être absolue (lien HATEOAS) ou relative (/api/...)
    const fullUrl = url.startsWith('http') ? url : `${this.baseUrl}${url}`;
    return this.http.get<T>(fullUrl);
  }

  post<T>(url: string, body: unknown): Observable<T> {
    const fullUrl = url.startsWith('http') ? url : `${this.baseUrl}${url}`;
    return this.http.post<T>(fullUrl, body);
  }
}
```

---

## HATEOAS : Utiliser les `_links` de l'API

Le BFF retourne des liens hypermedia dans chaque réponse. **Angular ne doit jamais construire d'URLs en dur.**

### Exemple de réponse BFF

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "_links": {
    "self":    { "href": "/api/discogs/releases?page=1&pageSize=20" },
    "next":    { "href": "/api/discogs/releases?page=2&pageSize=20" },
    "prev":    null,
    "byGenre": { "href": "/api/discogs/genres" }
  }
}
```

### Consommer les liens dans un composant Angular

```typescript
// features/catalog/releases-list/releases-list.component.ts
@Component({ ... })
export class ReleasesListComponent {
  private readonly api = inject(ApiService);

  releases = signal<Release[]>([]);
  links = signal<HateoasLinks>({});

  loadPage(url: string): void {
    this.api.get<ReleasesPagedResponse>(url).subscribe(response => {
      this.releases.set(response.items);
      this.links.set(response._links);
    });
  }

  nextPage(): void {
    const next = this.links().next?.href;
    if (next) this.loadPage(next);           // URL fournie par le BFF
  }

  previousPage(): void {
    const prev = this.links().prev?.href;
    if (prev) this.loadPage(prev);
  }
}
```

> **Bonne pratique** : Ne jamais écrire `/api/discogs/releases?page=2` en dur dans Angular. Toujours utiliser le lien `next` fourni par le BFF. Cela garantit que si les URLs changent côté serveur, le frontend ne casse pas.

---

## Authentification

La gestion de l'authentification est centralisée dans le BFF (JWT ou Cookies sécurisés). Angular n'a pas accès direct aux secrets.

### Flux de connexion

```
Angular          BFF
  │── POST /api/customers/login ──────────→ │
  │                                         │ Vérifie credentials
  │← ── { token: "...", _links: {...} } ────│
  │                                         │
  │  (stocke le token en mémoire/signal)    │
  │                                         │
  │── GET /api/customers/account ──────────→│
  │   Authorization: Bearer <token>         │
  │← ── { name, email, _links: {...} } ────│
```

### Intercepteur d'authentification

```typescript
// core/interceptors/auth.interceptor.ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token();

  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  }
  return next(req);
};
```

---

## Modèles TypeScript (DTOs)

Les interfaces TypeScript doivent refléter exactement les DTOs retournés par le BFF. **Ne pas inventer de champs qui n'existent pas dans l'API.**

```typescript
// shared/models/hateoas.model.ts
export interface HateoasLink {
  href: string;
}

export interface HateoasLinks {
  self?:      HateoasLink | null;
  next?:      HateoasLink | null;
  prev?:      HateoasLink | null;
  first?:     HateoasLink | null;
  last?:      HateoasLink | null;
  [key: string]: HateoasLink | null | undefined;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  _links: HateoasLinks;
}
```

```typescript
// features/catalog/models/release.model.ts
export interface Release {
  id: string;           // ULID
  title: string;
  artist: string;
  year: number;
  genre: string;
  format: string;
  _links?: HateoasLinks;
}
```

---

## Gestion des erreurs

Le BFF retourne des erreurs standardisées (RFC 7807 Problem Details). Traiter les erreurs globalement via un intercepteur.

```typescript
// core/interceptors/error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401: // Non authentifié → rediriger vers /login
          inject(Router).navigate(['/login']);
          break;
        case 403: // Non autorisé → afficher message
          break;
        case 422: // Erreur de validation → afficher dans le formulaire
          break;
        case 500: // Erreur serveur → afficher message générique
          break;
      }
      return throwError(() => error);
    })
  );
};
```

---

## Conventions de code

### Nommage

| Élément | Convention | Exemple |
|---|---|---|
| Composant | `kebab-case` (fichier) + `PascalCase` (classe) | `release-card.component.ts` |
| Service | `camelCase` suffixé `.service` | `auth.service.ts` |
| Signal | `camelCase` | `releases = signal<Release[]>([])` |
| Interface/Model | `PascalCase` | `Release`, `PagedResponse<T>` |
| URL de feature | `kebab-case` | `/catalog/releases` |

### À faire / À éviter

| À faire | À éviter |
|---|---|
| Utiliser les `_links` du BFF pour naviguer | Construire des URLs en dur |
| Typer toutes les réponses HTTP | Utiliser `any` |
| Utiliser les Signals Angular pour l'état | Utiliser des variables de classe classiques |
| Un service par domaine fonctionnel | Un service "God" pour tout |
| Composants Standalone | NgModules (déprécié dans ce projet) |

---

## Démarrage rapide pour un nouveau développeur

### Prérequis

- Node.js 22+ et npm
- Angular CLI : `npm install -g @angular/cli`
- Le backend qui tourne (voir [README.md](README.md))

### Lancer le frontend

```bash
# À la racine du projet Angular (à créer dans src/PlateformeLocationDisques.WebApp/)
cd src/PlateformeLocationDisques.WebApp

npm install
ng serve
# → http://localhost:4200
```

### Premier endpoint à explorer

Ouvre Scalar dans ton navigateur : `https://localhost:5001/scalar/v1`

Teste l'endpoint de recherche de releases :
```
GET /api/discogs/releases?page=1&pageSize=10
```

Observe la structure JSON retournée, notamment les `_links`. C'est ce format que tu vas consommer dans Angular.

---

## Checklist avant de soumettre du code frontend

1. [ ] Les URLs HTTP utilisent les `_links` fournis par le BFF (pas d'URLs en dur)
2. [ ] Toutes les réponses HTTP sont typées avec une interface TypeScript
3. [ ] L'état du composant est géré avec des `signal()`
4. [ ] Les erreurs HTTP sont gérées (soit via l'intercepteur global, soit localement)
5. [ ] Le composant est Standalone (`standalone: true`)
6. [ ] Aucune logique métier dans le template HTML (déléguée au composant ou au service)
7. [ ] Les noms de features Angular correspondent aux modules du BFF

---

## Liens utiles

- [README.md](README.md) — Démarrage du backend
- [architecture.md](architecture.md) — Architecture générale du projet
- [coding-guidelines.md](coding-guidelines.md) — Conventions côté backend
- API interactive : `https://localhost:5001/scalar/v1` (backend lancé)
