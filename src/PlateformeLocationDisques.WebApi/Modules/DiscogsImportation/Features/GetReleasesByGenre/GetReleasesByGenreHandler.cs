using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByGenre;

/// <summary>
/// Handler for GetReleasesByGenre query.
/// </summary>
public static class GetReleasesByGenreHandler
{
    public static async Task<GetReleasesByGenreResult> Handle(
        GetReleasesByGenre query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Apply search filter at the database level (optimized for EF Core)
        var filteredQuery = dbContext.Releases
            .AsNoTracking()
            .Where(r => r.Genres.Contains(query.Genre));

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        // Apply pagination and projection (BFF pattern)
        var items = await filteredQuery
            .OrderByDescending(r => r.Year)
            .ThenBy(r => r.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReleaseByGenreItemDto(
                Id: r.Id.ToString(),
                DiscogsId: r.DiscogsId,
                Title: r.Title,
                Year: r.Year,
                Country: r.Country,
                Genres: r.Genres.ToArray(),
                Styles: r.Styles.ToArray(),
                Artists: r.Artists.Select(a => a.Name).ToArray(), // Projection
                Thumb: r.Thumb,
                Format: r.Formats.FirstOrDefault() != null
                    ? r.Formats.OrderBy(f => f.Id).First().Name
                    : null
            ))
            .ToArrayAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new GetReleasesByGenreResult(
            Items: items,
            Genre: query.Genre,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages
        );
    }
}
