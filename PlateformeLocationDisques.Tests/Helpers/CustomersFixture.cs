using Alba;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.Customers.Infrastructure;

namespace PlateformeLocationDisques.Tests.Helpers;

public class CustomersFixture : IAsyncLifetime
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
                var customersDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<CustomersDbContext>));
                if (customersDescriptor != null) services.Remove(customersDescriptor);

                services.AddDbContext<CustomersDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
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
