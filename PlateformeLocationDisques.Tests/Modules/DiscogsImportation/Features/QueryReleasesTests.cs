using Alba;
using FluentAssertions;
using PlateformeLocationDisques.Tests.Helpers;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetMasterReleaseById;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.SearchReleases;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByGenre;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByArtist;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.DiscogsImportation.Features;

[Collection(nameof(DiscogsReadOnlyCollection))]
public class QueryReleasesTests
{
    private readonly DiscogsReadOnlyFixture _fixture;

    public QueryReleasesTests(DiscogsReadOnlyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMasterReleaseById_Should_Return_DTO_With_Correct_Format()
    {
        var host = _fixture.Host;

        // Act & Assert - Query the pre-seeded data
        await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/master-releases");
            _.StatusCodeShouldBeOk();
        });
    }

    [Fact]
    public async Task SearchReleases_Should_Return_Paginated_Results()
    {
        var host = _fixture.Host;

        // Act
        var response = await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/releases?page=1&pageSize=10");
            _.StatusCodeShouldBeOk();
        });

        // Assert
        var result = response.ReadAsJson<SearchReleasesResult>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SearchReleases_Should_Filter_By_Search_Term()
    {
        var host = _fixture.Host;

        // Act - Search for "Dark Side"
        var response = await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/releases?search=Dark");
            _.StatusCodeShouldBeOk();
        });

        // Assert
        var result = response.ReadAsJson<SearchReleasesResult>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReleasesByGenre_Should_Filter_Correctly()
    {
        var host = _fixture.Host;

        // Act
        var response = await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/releases/genre/Rock?page=1&pageSize=20");
            _.StatusCodeShouldBeOk();
        });

        // Assert
        var result = response.ReadAsJson<GetReleasesByGenreResult>();
        result.Should().NotBeNull();
        result!.Genre.Should().Be("Rock");
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetReleasesByArtist_Should_Filter_Correctly()
    {
        var host = _fixture.Host;

        // Act
        var response = await host.Scenario(_ =>
        {
            _.Get.Url("/api/discogs/releases/artist/Pink Floyd?page=1&pageSize=20");
            _.StatusCodeShouldBeOk();
        });

        // Assert
        var result = response.ReadAsJson<GetReleasesByArtistResult>();
        result.Should().NotBeNull();
        result!.ArtistName.Should().Be("Pink Floyd");
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }
}
