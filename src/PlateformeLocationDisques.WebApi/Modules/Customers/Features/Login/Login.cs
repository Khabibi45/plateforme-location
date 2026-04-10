using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.Customers.Infrastructure;
using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.Customers.Features.Login;

// 1. Command
public record LoginRequest(string Email, string Password);

// 2. Response DTO
public record LoginResponse(bool Success, string? Token, string? Message);

// 3. Handler
public static class LoginHandler
{
    public static async Task<LoginResponse> Handle(
        LoginRequest request, 
        CustomersDbContext dbContext)
    {
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email);

        if (customer == null || customer.PasswordHash != "hashed_" + request.Password)
        {
            return new LoginResponse(false, null, "Identifiants invalides");
        }

        // Simuler la génération d'un token JWT (BFF style)
        var token = "simulated_jwt_token_for_" + customer.Id;
        
        return new LoginResponse(true, token, "Connexion réussie");
    }
}

// 4. Endpoint
public static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/customers/login", async (LoginRequest request, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<LoginResponse>(request);
            return result.Success ? Results.Ok(result) : Results.Unauthorized();
        })
        .WithTags("Customers");
    }
}
