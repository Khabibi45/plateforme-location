using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllArtists;

/// <summary>
/// Handler for GetAllArtists query.
/// </summary>
public static class GetAllArtistsHandler
{
    public static async Task<GetAllArtistsResult> Handle(
        GetAllArtists query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Get all releases and master releases to extract artists
        var allReleases = await dbContext.Releases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allMasterReleases = await dbContext.MasterReleases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Combine artists from both releases and master releases
        var allArtists = allReleases
            .SelectMany(r => r.Artists)
            .Concat(allMasterReleases.SelectMany(m => m.Artists))
            .ToList();

        // Flatten all artists and count occurrences
        var artistGroups = allArtists
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(a => a.Count)
            .ThenBy(a => a.Name)
            .ToArray();

        // Map to DTOs with HATEOAS links
        var artistDtos = artistGroups.Select(a =>
        {
            var artistLinks = new Links();
            artistLinks.Add("self", $"/api/discogs/artists/{Uri.EscapeDataString(a.Name)}");
            artistLinks.AddArtistLink(a.Name);
            artistLinks.Add("releases", "/api/discogs/releases");

            return new ArtistDto(
                Name: a.Name,
                ReleaseCount: a.Count,
                Links: artistLinks
            );
        }).ToArray();

        // Create collection links
        var links = Links.CreateSelf("/api/discogs/artists");
        links.Add("releases", "/api/discogs/releases");
        links.Add("masterReleases", "/api/discogs/master-releases");
        links.Add("genres", "/api/discogs/genres");

        return new GetAllArtistsResult(
            Artists: artistDtos,
            TotalCount: artistDtos.Length,
            Links: links
        );
    }
}
