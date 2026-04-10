using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllMasterReleases;

/// <summary>
/// Endpoint to get all master releases with pagination and HATEOAS links.
/// </summary>
public static class GetAllMasterReleasesEndpoint
{
    public static void MapGetAllMasterReleases(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/master-releases", async (
            int page = 1,
            int pageSize = 20,
            string? search = null,
            IMessageBus bus = default!,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetAllMasterReleases(page, pageSize, search);
            var result = await bus.InvokeAsync<GetAllMasterReleasesResult>(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllMasterReleases")
        .WithTags("Discogs - Master Releases")
        .WithSummary("Get all imported master releases with pagination")
        .WithDescription("Returns a paginated list of all imported master releases with HATEOAS links for navigation and filtering.");
    }
}
