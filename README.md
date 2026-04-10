# Plateforme Location Disques

A modular monolith API for managing a music disc rental platform, built with .NET 10, Vertical Slice Architecture, and CQRS pattern using Wolverine.

## Architecture

- **Vertical Slice Architecture**: Features are organized by business capability, not technical layers
- **Modular Monolith**: Logical separation of modules (Customers, Discogs Importation) with shared infrastructure
- **CQRS**: Command/Query separation using Wolverine message bus
- **API-First & BFF**: Backend For Frontend pattern with optimized DTOs for the Angular client
- **PostgreSQL**: Primary data store with EF Core 10 and Npgsql
- **ULID**: Universally Unique Lexicographically Sortable Identifiers for primary keys

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/products/docker-desktop) (for PostgreSQL)

## Getting Started - Local Development

### 1. Start the Complete Environment

```bash
docker compose up -d --build
```

This starts:
1. **PostgreSQL 17 container**
   - **Database**: `discogs_db`
   - **User**: `discogs_user`
   - **Password**: `discogs_local_password`
   - **Port**: `5432` (Internal/External)
2. **Web API container**
   - **Environment**: `Development`
   - **HTTP**: `http://localhost:5000`
   - **HTTPS**: `https://localhost:5001`

The Web API will automatically:
- Wait for PostgreSQL to be healthy
- Connect to the database
- Create the schema and seed sample data

### 2. Manual Run (Alternative)

If you prefer to run the Web API locally (outside Docker) but keep PostgreSQL in Docker:

1. **Start only PostgreSQL**:
   ```bash
   docker compose up -d postgres
   ```

2. **Run the Application**:
   ```bash
   cd src/PlateformeLocationDisques.WebApi
   dotnet run
   ```

### 3. Explore the API

#### Scalar UI (Development only)
- Navigate to: `https://localhost:5001/scalar/v1`
- Interactive API documentation with request/response examples
- Test endpoints directly from the browser

#### OpenAPI Specification
- OpenAPI JSON: `https://localhost:5001/openapi/v1.json`
- Import into tools like Postman, Insomnia, or Bruno

#### Test Endpoints

**Get Master Release by ID**
```bash
# First, import a master release (optional, already seeded)
curl -X POST https://localhost:5001/api/discogs/import/master/1000

# Then retrieve it (use the ID from the import response or from seed data)
curl https://localhost:5001/api/discogs/master-releases/{id}
```

**Search Releases**
```bash
# General search
curl "https://localhost:5001/api/discogs/releases?search=Dark&page=1&pageSize=10"

# Search without filter
curl "https://localhost:5001/api/discogs/releases?page=1&pageSize=20"
```

**Get Releases by Genre**
```bash
curl "https://localhost:5001/api/discogs/releases/genre/Rock?page=1&pageSize=20"
```

**Get Releases by Artist**
```bash
curl "https://localhost:5001/api/discogs/releases/artist/Pink%20Floyd?page=1&pageSize=20"
```

## Sample Seeded Data

The database is automatically seeded with:
- **Master Release**: Pink Floyd - The Dark Side Of The Moon (1973)
  - 10 tracks
  - Genres: Rock
  - Styles: Psychedelic Rock, Progressive Rock
  - Community rating: 4.67/5
- **Release**: UK pressing on Harvest label (SHVL 804)
  - Format: LP, Vinyl, Gatefold
  - Community rating: 4.72/5

## Module Structure

### Discogs Importation Module

Located in `src/PlateformeLocationDisques.WebApi/Modules/DiscogsImportation/`

- **Domain**: Entities (MasterRelease, Release, ReleaseArtist, etc.)
- **Features**: Vertical slices with Command/Query, Handler, Endpoint
  - `ImportMasterRelease`: Import master releases from Discogs API
  - `GetMasterReleaseById`: Retrieve master release by ID
  - `GetReleaseById`: Retrieve specific release by ID
  - `SearchReleases`: Paginated search by title, artist, or catalog number
  - `GetReleasesByGenre`: Filter releases by genre
  - `GetReleasesByArtist`: Filter releases by artist name
- **Infrastructure**: DbContext, migrations, seeding
- **Adapters**: IDiscogsClient interface with fake and real implementations

## Testing

### Run All Tests

```bash
dotnet test
```

### Integration Tests

Tests use:
- **Alba**: For HTTP integration testing
- **EF Core InMemory**: For fast test database
- **FakeDiscogsClient**: Avoids consuming real Discogs API during tests

Test files are located in `PlateformeLocationDisques.Tests/Modules/{ModuleName}/Features/`

## Configuration

### appsettings.Development.json

```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DiscogsDb": "Host=localhost;Port=5432;Database=discogs_db;Username=discogs_user;Password=discogs_local_password"
  },
  "SeedDatabase": true,
  "Discogs": {
    "ApiToken": "FAKE_TOKEN_NOT_USED_IN_DEV"
  }
}
```

### appsettings.json (Production)

```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DiscogsDb": "YOUR_PRODUCTION_CONNECTION_STRING"
  },
  "SeedDatabase": false,
  "Discogs": {
    "ApiToken": "YOUR_REAL_DISCOGS_API_TOKEN"
  }
}
```

## Stopping the Development Environment

```bash
# Stop the application: Ctrl+C in the terminal

# Stop and remove PostgreSQL container
docker compose down

# Stop and remove container + volumes (deletes all data)
docker compose down -v
```

## Project Guidelines

- **architecture.md**: Architectural decisions and patterns
- **coding-guidelines.md**: Coding standards and best practices
- **testing-guidelines.md**: Testing strategy and conventions
- **src/PlateformeLocationDisques.WebApi/DomainAndModels/releases.md**: Discogs data model specification

## Technologies

- .NET 10
- ASP.NET Core Minimal APIs
- EF Core 10 with PostgreSQL (Npgsql)
- Wolverine (CQRS/Message Bus)
- ByteAether.Ulid (ULID generation)
- Alba (Integration testing)
- FluentAssertions (Test assertions)
- PostgreSQL 17

## License

[Your License Here]
