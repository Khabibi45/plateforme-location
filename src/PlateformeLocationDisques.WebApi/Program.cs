using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.Customers.Features.Login;
using PlateformeLocationDisques.WebApi.Modules.Customers.Features.Register;
using PlateformeLocationDisques.WebApi.Modules.Customers.Infrastructure;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.ImportMasterRelease;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetMasterReleaseById;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleaseById;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.SearchReleases;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByGenre;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByArtist;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllGenres;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllArtists;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllMasterReleases;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration EF Core - Customers Module
var useInMemoryDb = builder.Environment.IsEnvironment("Test") || 
                    builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);
builder.Services.AddDbContext<CustomersDbContext>(options =>
{
    if (useInMemoryDb)
    {
        options.UseInMemoryDatabase("CustomersDb");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("CustomersDb")
            ?? throw new InvalidOperationException("PostgreSQL connection string 'CustomersDb' is not configured.");
        options.UseNpgsql(connectionString);
    }
});

// Configuration EF Core - Discogs Importation Module
builder.Services.AddDbContext<DiscogsDbContext>(options =>
{
    if (useInMemoryDb)
    {
        options.UseInMemoryDatabase("DiscogsDb");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DiscogsDb")
            ?? throw new InvalidOperationException("PostgreSQL connection string 'DiscogsDb' is not configured.");
        options.UseNpgsql(connectionString);
    }
});

// Register Discogs API Client (Adapter Pattern)
// Use FakeDiscogsClient in Development, real client in Production
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDiscogsClient, FakeDiscogsClient>();
}
else
{
    builder.Services.AddHttpClient<IDiscogsClient, DiscogsApiClient>();
}

// Configuration Wolverine
builder.Host.UseWolverine(opts =>
{
    // Auto-découverte des handlers dans l'assemblage
    opts.UseEntityFrameworkCoreTransactions();
});

// Swagger / OpenAPI (Optionnel mais recommandé pour API First)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // .NET 10 style

var app = builder.Build();

// Database initialization
{
    using var scope = app.Services.CreateScope();
    var customersDb = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
    var discogsDb = scope.ServiceProvider.GetRequiredService<DiscogsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (!useInMemoryDb)
    {
        // Run migrations in non-development environments or if explicitly requested via environment variable
        var runMigrations = !app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("RunMigrations", false);
        if (runMigrations)
        {
            await customersDb.Database.MigrateAsync();
            await discogsDb.Database.MigrateAsync();
        }
        else
        {
            // In development, ensure database is created
            await customersDb.Database.EnsureCreatedAsync();
            await discogsDb.Database.EnsureCreatedAsync();
        }
    }
    else
    {
        // For InMemory database, ensure it's created
        await customersDb.Database.EnsureCreatedAsync();
        await discogsDb.Database.EnsureCreatedAsync();
    }

    // Seed database in development if configured (works for both InMemory and PostgreSQL)
    if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("SeedDatabase", false))
    {
        await DatabaseSeeder.SeedAsync(discogsDb, logger);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Enregistrement des Endpoints (Modular Monolith style)
// Customers Module
app.MapRegisterCustomer();
app.MapLogin();

// Discogs Importation Module
app.MapImportMasterRelease();
app.MapGetMasterReleaseById();
app.MapGetReleaseById();
app.MapSearchReleases();
app.MapGetReleasesByGenre();
app.MapGetReleasesByArtist();

// Browse endpoints with HATEOAS
app.MapGetAllMasterReleases();
app.MapGetAllGenres();
app.MapGetAllArtists();

app.MapGet("/", () => "Plateforme Location Disques API");

app.Run();
