using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllGenres;

/// <summary>
/// Handler for GetAllGenres query.
/// </summary>
public static class GetAllGenresHandler
{
    public static async Task<GetAllGenresResult> Handle(
        GetAllGenres query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Get all releases and master releases to extract genres
        var allReleases = await dbContext.Releases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allMasterReleases = await dbContext.MasterReleases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Combine genres from both releases and master releases
        var allGenres = allReleases
            .SelectMany(r => r.Genres)
            .Concat(allMasterReleases.SelectMany(m => m.Genres))
            .ToList();

        // Flatten all genres and count occurrences
        var genreGroups = allGenres
            .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Name)
            .ToArray();

        // Map to DTOs with HATEOAS links
        var genreDtos = genreGroups.Select(g =>
        {
            var genreLinks = new Links();
            genreLinks.Add("self", $"/api/discogs/genres/{Uri.EscapeDataString(g.Name)}");
            genreLinks.AddGenreLink(g.Name);
            genreLinks.Add("releases", "/api/discogs/releases");

            return new GenreDto(
                Name: g.Name,
                ReleaseCount: g.Count,
                Links: genreLinks
            );
        }).ToArray();

        // Create collection links
        var links = Links.CreateSelf("/api/discogs/genres");
        links.Add("releases", "/api/discogs/releases");
        links.Add("masterReleases", "/api/discogs/master-releases");
        links.Add("artists", "/api/discogs/artists");

        return new GetAllGenresResult(
            Genres: genreDtos,
            TotalCount: genreDtos.Length,
            Links: links
        );
    }
}
