using PlateformeLocationDisques.WebApi.Shared.Hypermedia;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetAllMasterReleases;

/// <summary>
/// Query to get all master releases with pagination.
/// </summary>
public record GetAllMasterReleases(int Page = 1, int PageSize = 20, string? SearchTerm = null);

/// <summary>
/// Result containing paginated master releases with HATEOAS links.
/// </summary>
public record GetAllMasterReleasesResult(
    MasterReleaseListItemDto[] Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    Links Links
);

/// <summary>
/// DTO for a master release item in the list with HATEOAS links.
/// </summary>
public record MasterReleaseListItemDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string[] Genres,
    string[] Artists,
    Links Links
);
