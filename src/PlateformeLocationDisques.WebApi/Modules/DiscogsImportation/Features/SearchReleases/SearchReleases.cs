using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.SearchReleases;

/// <summary>
/// Query to search releases by title, artist name, or catalog number.
/// </summary>
public record SearchReleases(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Paginated search results with HATEOAS links.
/// </summary>
public record SearchReleasesResult(
    ReleaseSearchItemDto[] Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    Links Links
);

/// <summary>
/// Lightweight DTO for search results list with HATEOAS links.
/// </summary>
public record ReleaseSearchItemDto(
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
