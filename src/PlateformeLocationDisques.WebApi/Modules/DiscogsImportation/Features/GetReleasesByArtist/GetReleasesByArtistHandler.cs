using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByArtist;

/// <summary>
/// Handler for GetReleasesByArtist query.
/// </summary>
public static class GetReleasesByArtistHandler
{
    public static async Task<GetReleasesByArtistResult> Handle(
        GetReleasesByArtist query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Apply search filter at the database level (optimized for EF Core)
        var artistName = query.ArtistName;
        var filteredQuery = dbContext.Releases
            .AsNoTracking()
            .Where(r => r.Artists.Any(a =>
                a.Name.Contains(artistName) ||
                (a.Anv != null && a.Anv.Contains(artistName))
            ));

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        // Apply pagination and projection (BFF pattern)
        var items = await filteredQuery
            .OrderByDescending(r => r.Year)
            .ThenBy(r => r.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReleaseByArtistItemDto(
                Id: r.Id.ToString(),
                DiscogsId: r.DiscogsId,
                Title: r.Title,
                Year: r.Year,
                Country: r.Country,
                Genres: r.Genres.ToArray(),
                Styles: r.Styles.ToArray(),
                Thumb: r.Thumb,
                Format: r.Formats.FirstOrDefault() != null
                    ? r.Formats.OrderBy(f => f.Id).First().Name
                    : null
            ))
            .ToArrayAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new GetReleasesByArtistResult(
            Items: items,
            ArtistName: query.ArtistName,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages
        );
    }
}
