namespace PlateformeLocationDisques.WebApi.Modules.Customers.Domain;

public class Customer
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MembershipType { get; set; } = "Standard";
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
}
