using Alba;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlateformeLocationDisques.Tests.Helpers;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.ImportMasterRelease;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using Xunit;

namespace PlateformeLocationDisques.Tests.Modules.DiscogsImportation.Features;

[Collection(nameof(DiscogsIsolatedCollection))]
public class ImportMasterReleaseTests
{
    private readonly DiscogsIsolatedFixture _fixture;

    public ImportMasterReleaseTests(DiscogsIsolatedFixture fixture)
    {
        _fixture = fixture;
    }
    [Fact]
    public async Task ImportMasterRelease_Should_Import_From_FakeClient_Successfully()
    {
        var host = _fixture.Host;

        // Act - Import master release with ID 1 (Pink Floyd from FakeDiscogsClient)
        var response = await host.Scenario(_ =>
        {
            _.Post.Url("/api/discogs/import/master/1");
            _.StatusCodeShouldBeOk();
        });

        // Assert - Verify BFF contract
        var result = response.ReadAsJson<MasterReleaseImported>();
        result.Should().NotBeNull();
        result!.DiscogsId.Should().Be(1);
        result.Title.Should().Be("The Dark Side Of The Moon");
        result.AlreadyExisted.Should().BeFalse();
        result.Id.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ImportMasterRelease_Should_Return_Existing_If_Already_Imported()
    {
        var host = _fixture.Host;

        // Act - Import twice
        var firstResponse = await host.Scenario(_ =>
        {
            _.Post.Url("/api/discogs/import/master/1");
            _.StatusCodeShouldBeOk();
        });

        var firstResult = firstResponse.ReadAsJson<MasterReleaseImported>();

        var secondResponse = await host.Scenario(_ =>
        {
            _.Post.Url("/api/discogs/import/master/1");
            _.StatusCodeShouldBeOk();
        });

        // Assert - Second import should return the same ID and AlreadyExisted = true
        var secondResult = secondResponse.ReadAsJson<MasterReleaseImported>();
        secondResult!.Id.Should().Be(firstResult!.Id);
        secondResult.DiscogsId.Should().Be(1);
        secondResult.AlreadyExisted.Should().BeTrue();
    }

    [Fact]
    public async Task ImportMasterRelease_Should_Persist_Complete_Data()
    {
        var host = _fixture.Host;

        // Act - Import master release
        var response = await host.Scenario(_ =>
        {
            _.Post.Url("/api/discogs/import/master/1");
            _.StatusCodeShouldBeOk();
        });

        var result = response.ReadAsJson<MasterReleaseImported>();

        // Assert - Verify data was persisted correctly
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscogsDbContext>();
        var masterRelease = await dbContext.MasterReleases
            .FirstOrDefaultAsync(m => m.DiscogsId == 1);

        masterRelease.Should().NotBeNull();
        masterRelease!.Title.Should().Be("The Dark Side Of The Moon");
        masterRelease.Year.Should().Be(1973);
        masterRelease.Genres.Should().Contain("Rock");
        masterRelease.Artists.Should().NotBeEmpty();
        masterRelease.Artists.First().Name.Should().Be("Pink Floyd");
        masterRelease.Tracklist.Should().HaveCount(10);
    }
}
