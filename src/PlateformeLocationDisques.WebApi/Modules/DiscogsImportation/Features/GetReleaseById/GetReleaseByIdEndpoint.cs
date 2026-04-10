using ByteAether.Ulid;
using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleaseById;

/// <summary>
/// Endpoint for retrieving a release by ID.
/// </summary>
public static class GetReleaseByIdEndpoint
{
    public static void MapGetReleaseById(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases/{id}",
            async (string id, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                if (!Ulid.TryParse(id, System.Globalization.CultureInfo.InvariantCulture, out var ulid))
                {
                    return Results.BadRequest(new { error = "Invalid ID format" });
                }

                var query = new GetReleaseById(ulid);
                var result = await bus.InvokeAsync<ReleaseDto?>(query, cancellationToken);

                return result != null
                    ? Results.Ok(result)
                    : Results.NotFound(new { error = "Release not found" });
            })
            .WithName("GetReleaseById")
            .WithTags("Discogs Importation")
            .WithSummary("Get a release by its internal ID")
            .Produces<ReleaseDto>(200)
            .Produces(400)
            .Produces(404);
    }
}
