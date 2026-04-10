using Wolverine;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByGenre;

/// <summary>
/// Endpoint for getting releases by genre.
/// </summary>
public static class GetReleasesByGenreEndpoint
{
    public static void MapGetReleasesByGenre(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/discogs/releases/genre/{genre}",
            async (
                string genre,
                int? page,
                int? pageSize,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var query = new GetReleasesByGenre(
                    Genre: genre,
                    Page: page ?? 1,
                    PageSize: pageSize is > 0 and <= 100 ? pageSize.Value : 20
                );

                var result = await bus.InvokeAsync<GetReleasesByGenreResult>(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetReleasesByGenre")
            .WithTags("Discogs Importation")
            .WithSummary("Get releases filtered by genre")
            .Produces<GetReleasesByGenreResult>(200);
    }
}
