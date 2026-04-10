using ByteAether.Ulid;
using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetMasterReleaseById;

/// <summary>
/// Endpoint for retrieving a master release by ID.
/// </summary>
public static class GetMasterReleaseByIdEndpoint
{
    public static void MapGetMasterReleaseById(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/master-releases/{id}",
            async (string id, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                if (!Ulid.TryParse(id, System.Globalization.CultureInfo.InvariantCulture, out var ulid))
                {
                    return Results.BadRequest(new { error = "Invalid ID format" });
                }

                var query = new GetMasterReleaseById(ulid);
                var result = await bus.InvokeAsync<MasterReleaseDto?>(query, cancellationToken);

                return result != null
                    ? Results.Ok(result)
                    : Results.NotFound(new { error = "Master release not found" });
            })
            .WithName("GetMasterReleaseById")
            .WithTags("Discogs Importation")
            .WithSummary("Get a master release by its internal ID")
            .Produces<MasterReleaseDto>(200)
            .Produces(400)
            .Produces(404);
    }
}
