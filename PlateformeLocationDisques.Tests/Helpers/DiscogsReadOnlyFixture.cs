using Alba;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.Tests.Helpers;

public class DiscogsReadOnlyFixture : IAsyncLifetime
{
    private IAlbaHost? _host;

    public IAlbaHost Host => _host ?? throw new InvalidOperationException("Fixture not initialized");

    public async Task InitializeAsync()
    {
        var dbName = Guid.NewGuid().ToString();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        _host = await AlbaHost.For<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var discogsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<DiscogsDbContext>));
                if (discogsDescriptor != null) services.Remove(discogsDescriptor);

                services.AddDbContext<DiscogsDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                var clientDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDiscogsClient));
                if (clientDescriptor != null) services.Remove(clientDescriptor);
                services.AddSingleton<IDiscogsClient, FakeDiscogsClient>();
            });
        });

        await SeedDataAsync();
    }

    private async Task SeedDataAsync()
    {
        await _host!.Scenario(_ =>
        {
            _.Post.Url("/api/discogs/import/master/1");
            _.StatusCodeShouldBeOk();
        });
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.DisposeAsync();
        }
    }
}
