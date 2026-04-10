using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.ImportMasterRelease;

/// <summary>
/// Endpoint for importing a master release from Discogs.
/// Following Vertical Slice Architecture, this endpoint is a simple gateway to the Wolverine message bus.
/// </summary>
public static class ImportMasterReleaseEndpoint
{
    public static void MapImportMasterRelease(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/discogs/import/master/{discogsId:int}",
            async (int discogsId, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                // No logic here - just dispatch to Wolverine
                var command = new ImportMasterRelease(discogsId);
                var result = await bus.InvokeAsync<MasterReleaseImported>(command, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("ImportMasterRelease")
            .WithTags("Discogs Importation")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Import a master release from Discogs by its ID";
                operation.Description = "Fetches a master release from the Discogs API and stores it in the database. " +
                                       "Returns the imported master release information. " +
                                       "If the master release already exists, it returns the existing record.";
                return operation;
            })
            .Produces<MasterReleaseImported>(200)
            .Produces(400);
    }
}
