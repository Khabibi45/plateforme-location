namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleasesByGenre;

/// <summary>
/// Query to get releases filtered by genre.
/// </summary>
public record GetReleasesByGenre(
    string Genre,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Paginated genre filter results.
/// </summary>
public record GetReleasesByGenreResult(
    ReleaseByGenreItemDto[] Items,
    string Genre,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Lightweight DTO for genre filter results.
/// </summary>
public record ReleaseByGenreItemDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string? Country,
    string[] Genres,
    string[] Styles,
    string[] Artists,
    string? Thumb,
    string? Format
);
