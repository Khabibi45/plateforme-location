using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllArtists;

/// <summary>
/// Query to get all distinct artists available in the catalog.
/// </summary>
public record GetAllArtists;

/// <summary>
/// Result containing all available artists with HATEOAS links.
/// </summary>
public record GetAllArtistsResult(
    ArtistDto[] Artists,
    int TotalCount,
    Links Links
);

/// <summary>
/// DTO for an artist with count and HATEOAS links.
/// </summary>
public record ArtistDto(
    string Name,
    int ReleaseCount,
    Links Links
);
