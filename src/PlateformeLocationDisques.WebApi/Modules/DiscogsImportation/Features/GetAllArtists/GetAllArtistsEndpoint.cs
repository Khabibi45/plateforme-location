using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllArtists;

/// <summary>
/// Endpoint to get all available artists with HATEOAS links.
/// </summary>
public static class GetAllArtistsEndpoint
{
    public static void MapGetAllArtists(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/artists", async (
            IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllArtists();
            var result = await bus.InvokeAsync<GetAllArtistsResult>(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllArtists")
        .WithTags("Discogs - Browse")
        .WithSummary("Get all available artists")
        .WithDescription("Returns a list of all distinct artists in the catalog with release counts and HATEOAS links for filtering.");
    }
}
