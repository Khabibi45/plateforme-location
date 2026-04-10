# Developer Quick Start Guide

Fast setup and daily debugging for local development.

## Prerequisites

- .NET 10 SDK
- Docker (PostgreSQL container)
- Rider IDE

## Daily Workflow

### 1. Start PostgreSQL

```bash
docker compose up -d postgres
```

Credentials (from `docker-compose.yml`):
- **Host**: `localhost:5432`
- **Databases**: `discogs_db`, `customers_db`
- **User**: `discogs_user`
- **Password**: `discogs_local_password`

### 2. Launch the Application

**Option A: Via Rider (Recommended)**
- Open `src/PlateformeLocationDisques.WebApi/Properties/launchSettings.json`
- Click the green play button or press `Shift+F10`
- App launches at `http://localhost:5078/scalar` (auto-opens Scalar UI)

**Option B: Via CLI**
```bash
cd src/PlateformeLocationDisques.WebApi
dotnet run
```

### 3. Access the API

- **Scalar UI** (interactive docs): `http://localhost:5078/scalar`
- **OpenAPI JSON**: `http://localhost:5078/openapi/v1.json`

## Database Debugging in Rider

### View Tables

1. Open **Database** tool (right panel or View → Tool Windows → Database)
2. Click your PostgreSQL connection
3. **Important**: Check "All Schemas" checkbox (top of Database panel)
4. Expand connection → Schemas:
   - `discogs` schema → `master_releases`, `releases` tables
   - `customers` schema → `customers` table
5. Right-click connection → **Refresh** to reload

### Run SQL Queries

- Double-click any table to preview data
- Click **SQL** tab at bottom to write custom queries
- Example: `SELECT * FROM discogs.master_releases LIMIT 10;`

## Configuration

**Development** (`appsettings.Development.json`):
- Uses local PostgreSQL (not in-memory)
- Auto-seeds sample data on startup
- Scalar UI enabled

**Tests** (xUnit):
- Use in-memory database (configured in test setup)
- No external dependencies needed
- Run: `dotnet test`

## Common Tasks

### Run Tests
```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter FullyQualifiedName~BrowseReleasesTests
```

### Check Database Connection
- Verify PostgreSQL is running: `docker compose ps`
- Check connection string in `appsettings.Development.json`
- Verify credentials match `docker-compose.yml`

### Reset Database
```bash
# Stop and remove all data
docker compose down -v

# Restart PostgreSQL
docker compose up -d postgres
```

### View Application Logs
- Rider: Check **Run** tool window at bottom
- Console output shows EF Core SQL queries (configured in appsettings)

## Troubleshooting

**Tables not visible in Rider DB tool?**
- Check "All Schemas" checkbox in Database panel
- Right-click connection → Refresh
- Verify app ran successfully (check logs for `EnsureCreatedAsync`)

**Connection refused?**
- Ensure PostgreSQL container is running: `docker compose ps`
- Verify port 5432 is not blocked
- Check credentials in `appsettings.Development.json`

**Port 5078 already in use?**
- Edit `launchSettings.json` and change `applicationUrl` port
- Or: `lsof -i :5078` and kill the process

## Architecture Notes

- **Modular Monolith**: Customers and Discogs Importation modules
- **Vertical Slice**: Each feature is self-contained (Command, Handler, Endpoint)
- **CQRS**: Wolverine message bus for commands/queries
- **Schemas**: Each module has its own PostgreSQL schema for isolation

## Useful Links

- `architecture.md` - Design decisions
- `coding-guidelines.md` - Code standards
- `testing-guidelines.md` - Test conventions
