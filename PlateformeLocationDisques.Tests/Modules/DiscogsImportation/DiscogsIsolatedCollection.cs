using PlateformeLocationDisques.Tests.Helpers;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.DiscogsImportation;

[CollectionDefinition(nameof(DiscogsIsolatedCollection))]
public class DiscogsIsolatedCollection : ICollectionFixture<DiscogsIsolatedFixture>
{
}
