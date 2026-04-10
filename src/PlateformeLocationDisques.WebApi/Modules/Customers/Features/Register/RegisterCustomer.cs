using PlateformeLocationDisques.WebApi.Modules.Customers.Domain;
using PlateformeLocationDisques.WebApi.Modules.Customers.Infrastructure;
using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.Customers.Features.Register;

// 1. Command
public record RegisterCustomer(string Email, string Password, string Name);

// 2. Handler (Wolverine style: static or instance)
public static class RegisterCustomerHandler
{
    public static async Task<CustomerRegistered> Handle(
        RegisterCustomer command, 
        CustomersDbContext dbContext)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed_" + command.Password, // Simulation hash
            Name = command.Name
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        return new CustomerRegistered(customer.Id);
    }
}

// 3. Event (Optional, for Wolverine side-effects)
public record CustomerRegistered(Guid Id);

// 4. Endpoint (Minimal API)
public static class RegisterCustomerEndpoint
{
    public static void MapRegisterCustomer(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/customers/register", async (RegisterCustomer command, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<CustomerRegistered>(command);
            return Results.Ok(result);
        })
        .WithTags("Customers");
    }
}
