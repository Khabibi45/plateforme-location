using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;
using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.SearchReleases;

/// <summary>
/// Handler for SearchReleases query.
/// </summary>
public static class SearchReleasesHandler
{
    public static async Task<SearchReleasesResult> Handle(
        SearchReleases query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Apply search filter at the database level (optimized for EF Core)
        var filteredQuery = dbContext.Releases
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm;
            // Note: In a real PostgreSQL environment with jsonb, 
            // the artist and label search might need specific EF Core function mappings
            // if they are not automatically translated from the LINQ expressions.
            filteredQuery = filteredQuery.Where(r =>
                r.Title.Contains(searchTerm) ||
                r.Artists.Any(a => a.Name.Contains(searchTerm)) ||
                r.Labels.Any(l => l.CatalogNumber.Contains(searchTerm))
            );
        }

        // Load all matching releases to apply client-side filtering (EF Core InMemory compatibility)
        var allMatchingReleases = await filteredQuery.ToListAsync(cancellationToken);

        // Client-side filtering for case-insensitive search
        var filtered = allMatchingReleases.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            filtered = filtered.Where(r =>
                r.Title.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Artists.Any(a => a.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                r.Labels.Any(l => l.CatalogNumber != null && l.CatalogNumber.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        var totalCount = filtered.Count();

        // Apply pagination and mapping with HATEOAS links
        var pagedReleases = filtered
            .OrderByDescending(r => r.ImportedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

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

            return new ReleaseSearchItemDto(
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
        var additionalParams = !string.IsNullOrWhiteSpace(query.SearchTerm)
            ? new Dictionary<string, string> { ["search"] = query.SearchTerm }
            : null;

        var links = LinkBuilder.CreatePaginationLinks(
            "/api/discogs/releases",
            query.Page,
            query.PageSize,
            totalPages,
            additionalParams
        );

        // Add browse links
        links.AddBrowseLinks();

        return new SearchReleasesResult(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages,
            Links: links
        );
    }
}
