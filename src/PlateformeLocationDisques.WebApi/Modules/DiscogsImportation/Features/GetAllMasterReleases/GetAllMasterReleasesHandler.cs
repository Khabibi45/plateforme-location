using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllMasterReleases;

/// <summary>
/// Handler for GetAllMasterReleases query.
/// </summary>
public static class GetAllMasterReleasesHandler
{
    public static async Task<GetAllMasterReleasesResult> Handle(
        GetAllMasterReleases query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Get all master releases
        var allMasterReleases = await dbContext.MasterReleases
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Apply search filter if provided
        var filtered = allMasterReleases.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            filtered = filtered.Where(m =>
                m.Title.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Artists.Any(a => a.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        var totalCount = filtered.Count();

        // Apply pagination
        var pagedMasters = filtered
            .OrderByDescending(m => m.ImportedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

        // Map to DTOs with HATEOAS links
        var items = pagedMasters.Select(m =>
        {
            var itemLinks = new Links();
            itemLinks.Add("self", $"/api/discogs/master-releases/{m.Id}");

            // Add genre filter links
            if (m.Genres.Any())
            {
                itemLinks.AddGenreLink(m.Genres.First());
            }

            // Add artist filter links
            if (m.Artists.Any())
            {
                itemLinks.AddArtistLink(m.Artists.First().Name);
            }

            return new MasterReleaseListItemDto(
                Id: m.Id.ToString(),
                DiscogsId: m.DiscogsId,
                Title: m.Title,
                Year: m.Year,
                Genres: m.Genres.ToArray(),
                Artists: m.Artists.Select(a => a.Name).ToArray(),
                Links: itemLinks
            );
        }).ToArray();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        // Create pagination links
        var additionalParams = !string.IsNullOrWhiteSpace(query.SearchTerm)
            ? new Dictionary<string, string> { ["search"] = query.SearchTerm }
            : null;

        var links = LinkBuilder.CreatePaginationLinks(
            "/api/discogs/master-releases",
            query.Page,
            query.PageSize,
            totalPages,
            additionalParams
        );

        // Add browse links
        links.AddBrowseLinks();
        links.Add("releases", "/api/discogs/releases");

        return new GetAllMasterReleasesResult(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages,
            Links: links
        );
    }
}
