using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllReleases;

/// <summary>
/// Query to get all releases with pagination.
/// </summary>
public record GetAllReleases(int Page = 1, int PageSize = 20);

/// <summary>
/// Result containing paginated releases with HATEOAS links.
/// </summary>
public record GetAllReleasesResult(
    ReleaseListItemDto[] Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    Links Links
);

/// <summary>
/// DTO for a release item in the list with HATEOAS links.
/// </summary>
public record ReleaseListItemDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string? Country,
    string[] Genres,
    string[] Artists,
    string? Thumb,
    string? Format,
    Links Links
);
