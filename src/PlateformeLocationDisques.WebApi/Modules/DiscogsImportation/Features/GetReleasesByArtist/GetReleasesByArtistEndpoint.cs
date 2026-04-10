using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByArtist;

/// <summary>
/// Endpoint for getting releases by artist name.
/// </summary>
public static class GetReleasesByArtistEndpoint
{
    public static void MapGetReleasesByArtist(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases/artist/{artistName}",
            async (
                string artistName,
                int? page,
                int? pageSize,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var query = new GetReleasesByArtist(
                    ArtistName: artistName,
                    Page: page ?? 1,
                    PageSize: pageSize is > 0 and <= 100 ? pageSize.Value : 20
                );

                var result = await bus.InvokeAsync<GetReleasesByArtistResult>(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetReleasesByArtist")
            .WithTags("Discogs Importation")
            .WithSummary("Get releases filtered by artist name")
            .Produces<GetReleasesByArtistResult>(200);
    }
}
