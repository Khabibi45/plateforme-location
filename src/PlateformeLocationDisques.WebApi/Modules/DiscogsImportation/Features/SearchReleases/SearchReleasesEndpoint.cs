using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.SearchReleases;

/// <summary>
/// Endpoint for searching releases.
/// </summary>
public static class SearchReleasesEndpoint
{
    public static void MapSearchReleases(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases",
            async (
                string? search,
                int? page,
                int? pageSize,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var query = new SearchReleases(
                    SearchTerm: search,
                    Page: page ?? 1,
                    PageSize: pageSize is > 0 and <= 100 ? pageSize.Value : 20
                );

                var result = await bus.InvokeAsync<SearchReleasesResult>(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchReleases")
            .WithTags("Discogs Importation")
            .WithSummary("Search releases by title, artist, or catalog number")
            .Produces<SearchReleasesResult>(200);
    }
}
