using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetMasterReleaseById;

/// <summary>
/// Query to get a master release by its internal ID.
/// </summary>
public record GetMasterReleaseById(Ulid Id);

/// <summary>
/// DTO for master release response (BFF pattern - only return what the client needs).
/// </summary>
public record MasterReleaseDto(
    string Id,
    int DiscogsId,
    string Title,
    int Year,
    string Status,
    string[] Genres,
    string[] Styles,
    MasterReleaseArtistDto[] Artists,
    MasterReleaseTrackDto[] Tracklist,
    string? Thumb,
    string? Notes,
    int? NumForSale,
    decimal? LowestPrice,
    MasterReleaseCommunityDto? Community,
    DateTime ImportedAt
);

public record MasterReleaseArtistDto(
    string Name,
    string? Anv,
    string? Role
);

public record MasterReleaseTrackDto(
    string Position,
    string Title,
    string? Duration,
    string Type
);

public record MasterReleaseCommunityDto(
    int Want,
    int Have,
    decimal? RatingAverage,
    int? RatingCount
);
