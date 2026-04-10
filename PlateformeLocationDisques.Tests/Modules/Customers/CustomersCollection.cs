using PlateformeLocationDisques.Tests.Helpers;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.Customers;

[CollectionDefinition(nameof(CustomersCollection))]
public class CustomersCollection : ICollectionFixture<CustomersFixture>
{
}
