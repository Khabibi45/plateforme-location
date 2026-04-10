namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByArtist;

/// <summary>
/// Query to get releases filtered by artist name.
/// </summary>
public record GetReleasesByArtist(
    string ArtistName,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Paginated artist filter results.
/// </summary>
public record GetReleasesByArtistResult(
    ReleaseByArtistItemDto[] Items,
    string ArtistName,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Lightweight DTO for artist filter results.
/// </summary>
public record ReleaseByArtistItemDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string? Country,
    string[] Genres,
    string[] Styles,
    string? Thumb,
    string? Format
);
