using Alba;
using FluentAssertions;
using PlateformeLocationDisques.Tests.Helpers;
using PlateformeLocationDisques.WebApi.Modules.Customers.Features.Login;
using PlateformeLocationDisques.WebApi.Modules.Customers.Features.Register;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.Customers.Features;

[Collection(nameof(CustomersCollection))]
public class CustomersFeaturesTests
{
    private readonly CustomersFixture _fixture;

    public CustomersFeaturesTests(CustomersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_And_Login_Should_Work_Together()
    {
        var host = _fixture.Host;

        // 2. Register a new customer
        var registerCommand = new RegisterCustomer("test@example.com", "P@ssword123", "Test User");
        var registerResponse = await host.Scenario(_ =>
        {
            _.Post.Json(registerCommand).ToUrl("/api/customers/register");
            _.StatusCodeShouldBeOk();
        });
        
        var registered = registerResponse.ReadAsJson<CustomerRegistered>();
        registered.Should().NotBeNull();
        registered!.Id.Should().NotBeEmpty();

        // 3. Login with the new customer
        var loginRequest = new LoginRequest("test@example.com", "P@ssword123");
        var loginResponse = await host.Scenario(_ =>
        {
            _.Post.Json(loginRequest).ToUrl("/api/customers/login");
            _.StatusCodeShouldBeOk();
        });
        
        var loginResult = loginResponse.ReadAsJson<LoginResponse>();
        loginResult!.Success.Should().BeTrue();
        loginResult.Token.Should().NotBeNullOrEmpty();
        loginResult.Message.Should().Be("Connexion réussie");
    }

    [Fact]
    public async Task Login_Should_Fail_With_Wrong_Credentials()
    {
        var host = _fixture.Host;

        // Act
        var loginRequest = new LoginRequest("nonexistent@example.com", "wrongpass");
        await host.Scenario(_ =>
        {
            _.Post.Json(loginRequest).ToUrl("/api/customers/login");
            _.StatusCodeShouldBe(401);
        });
    }
}
