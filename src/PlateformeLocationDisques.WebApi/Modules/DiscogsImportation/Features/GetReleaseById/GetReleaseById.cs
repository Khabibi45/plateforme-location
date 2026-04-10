using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleaseById;

/// <summary>
/// Query to get a release by its internal ID.
/// </summary>
public record GetReleaseById(Ulid Id);

/// <summary>
/// DTO for release response (BFF pattern).
/// </summary>
public record ReleaseDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string? Country,
    string Status,
    string[] Genres,
    string[] Styles,
    ReleaseArtistDto[] Artists,
    ReleaseLabelDto[] Labels,
    ReleaseFormatDto[] Formats,
    ReleaseTrackDto[] Tracklist,
    string? Thumb,
    string? Notes,
    string? MasterReleaseId,
    int? MasterDiscogsId,
    ReleaseCommunityDto? Community,
    DateTime ImportedAt
);

public record ReleaseArtistDto(
    string Name,
    string? Anv,
    string? Role
);

public record ReleaseLabelDto(
    string Name,
    string CatalogNumber
);

public record ReleaseFormatDto(
    string Name,
    string Quantity,
    string[] Descriptions
);

public record ReleaseTrackDto(
    string Position,
    string Title,
    string? Duration,
    string Type
);

public record ReleaseCommunityDto(
    int Want,
    int Have,
    decimal? RatingAverage,
    int? RatingCount
);
