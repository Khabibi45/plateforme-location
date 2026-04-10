using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.Customers.Domain;

namespace PlateformeLocationDisques.WebApi.Modules.Customers.Infrastructure;

public class CustomersDbContext : DbContext
{
    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");
        
        var customer = modelBuilder.Entity<Customer>();
        customer.HasKey(x => x.Id);
        customer.Property(x => x.Email).IsRequired().HasMaxLength(255);
        customer.HasIndex(x => x.Email).IsUnique();
    }
}
