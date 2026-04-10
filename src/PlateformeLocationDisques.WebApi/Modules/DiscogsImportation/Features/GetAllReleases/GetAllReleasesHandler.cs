using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllReleases;

/// <summary>
/// Handler for GetAllReleases query.
/// </summary>
public static class GetAllReleasesHandler
{
    public static async Task<GetAllReleasesResult> Handle(
        GetAllReleases query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Get all releases with pagination
        var allReleases = await dbContext.Releases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = allReleases.Count;

        // Apply pagination
        var pagedReleases = allReleases
            .OrderByDescending(r => r.ImportedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

        // Map to DTOs with HATEOAS links
        var items = pagedReleases.Select(r =>
        {
            var itemLinks = new Links();
            itemLinks.Add("self", $"/api/discogs/releases/{r.Id}");

            if (r.MasterReleaseId.HasValue)
            {
                itemLinks.Add("master", $"/api/discogs/master-releases/{r.MasterReleaseId.Value}");
            }

            // Add genre filter links
            if (r.Genres.Any())
            {
                itemLinks.AddGenreLink(r.Genres.First());
            }

            // Add artist filter links
            if (r.Artists.Any())
            {
                itemLinks.AddArtistLink(r.Artists.First().Name);
            }

            return new ReleaseListItemDto(
                Id: r.Id.ToString(),
                DiscogsId: r.DiscogsId,
                Title: r.Title,
                Year: r.Year,
                Country: r.Country,
                Genres: r.Genres.ToArray(),
                Artists: r.Artists.Select(a => a.Name).ToArray(),
                Thumb: r.Thumb,
                Format: r.Formats.Any()
                    ? r.Formats.OrderBy(f => f.Id).First().Name
                    : null,
                Links: itemLinks
            );
        }).ToArray();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        // Create pagination links
        var links = LinkBuilder.CreatePaginationLinks(
            "/api/discogs/releases",
            query.Page,
            query.PageSize,
            totalPages
        );

        // Add browse links
        links.AddBrowseLinks();

        return new GetAllReleasesResult(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages,
            Links: links
        );
    }
}
