using Alba;
using PlateformeLocationDisques.Tests.Helpers;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.DiscogsImportation.Features;

[Collection(nameof(DiscogsErrorCaseCollection))]
public class QueryReleasesErrorCaseTests
{
    private readonly DiscogsErrorCaseFixture _fixture;

    public QueryReleasesErrorCaseTests(DiscogsErrorCaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMasterReleaseById_Should_Return_404_For_NonExistent_Id()
    {
        var host = _fixture.Host;

        // Act & Assert
        await host.Scenario(_ =>
        {
            _.Get.Url($"/api/discogs/master-releases/{ByteAether.Ulid.Ulid.New()}");
            _.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task GetMasterReleaseById_Should_Return_400_For_Invalid_Id()
    {
        var host = _fixture.Host;

        // Act & Assert
        await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/master-releases/invalid-id");
            _.StatusCodeShouldBe(400);
        });
    }
}
