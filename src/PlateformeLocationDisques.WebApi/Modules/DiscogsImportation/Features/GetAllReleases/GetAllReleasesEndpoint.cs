using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllReleases;

/// <summary>
/// Endpoint to get all releases with pagination and HATEOAS links.
/// </summary>
public static class GetAllReleasesEndpoint
{
    public static void MapGetAllReleases(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases", async (
            int page = 1,
            int pageSize = 20,
            IMessageBus bus = default!,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetAllReleases(page, pageSize);
            var result = await bus.InvokeAsync<GetAllReleasesResult>(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllReleases")
        .WithTags("Discogs - Releases")
        .WithSummary("Get all imported releases with pagination")
        .WithDescription("Returns a paginated list of all imported releases with HATEOAS links for navigation and filtering.");
    }
}
