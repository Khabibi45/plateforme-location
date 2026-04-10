using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllGenres;

/// <summary>
/// Endpoint to get all available genres with HATEOAS links.
/// </summary>
public static class GetAllGenresEndpoint
{
    public static void MapGetAllGenres(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/genres", async (
            IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllGenres();
            var result = await bus.InvokeAsync<GetAllGenresResult>(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllGenres")
        .WithTags("Discogs - Browse")
        .WithSummary("Get all available genres")
        .WithDescription("Returns a list of all distinct genres in the catalog with release counts and HATEOAS links for filtering.");
    }
}
