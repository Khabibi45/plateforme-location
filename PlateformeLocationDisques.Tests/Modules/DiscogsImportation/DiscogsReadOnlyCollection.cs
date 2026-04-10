using PlateformeLocationDisques.Tests.Helpers;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.DiscogsImportation;

[CollectionDefinition(nameof(DiscogsReadOnlyCollection))]
public class DiscogsReadOnlyCollection : ICollectionFixture<DiscogsReadOnlyFixture>
{
}
