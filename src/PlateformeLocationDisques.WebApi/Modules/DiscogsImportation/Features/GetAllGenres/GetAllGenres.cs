using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllGenres;

/// <summary>
/// Query to get all distinct genres available in the catalog.
/// </summary>
public record GetAllGenres;

/// <summary>
/// Result containing all available genres with HATEOAS links.
/// </summary>
public record GetAllGenresResult(
    GenreDto[] Genres,
    int TotalCount,
    Links Links
);

/// <summary>
/// DTO for a genre with count and HATEOAS links.
/// </summary>
public record GenreDto(
    string Name,
    int ReleaseCount,
    Links Links
);
