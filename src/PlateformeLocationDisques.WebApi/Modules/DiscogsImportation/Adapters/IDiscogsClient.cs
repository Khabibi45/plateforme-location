namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;

public interface IDiscogsClient
{
    Task<DiscogsResult<DiscogsMasterReleaseDto>> GetMasterReleaseAsync(int masterId, CancellationToken cancellationToken = default);
    Task<DiscogsResult<DiscogsReleaseDto>> GetReleaseAsync(int releaseId, CancellationToken cancellationToken = default);
}

public record DiscogsMasterReleaseDto(
    int Id,
    string Title,
    int Year,
    string Status,
    string Uri,
    string ResourceUrl,
    string DataQuality,
    int? MainRelease,
    string? MainReleaseUrl,
    int? MostRecentRelease,
    string? MostRecentReleaseUrl,
    string? VersionsUrl,
    int? NumForSale,
    decimal? LowestPrice,
    string? Thumb,
    string? Notes,
    string[] Genres,
    string[] Styles,
    DiscogsArtistDto[] Artists,
    DiscogsTrackDto[] Tracklist,
    DiscogsImageDto[] Images,
    DiscogsVideoDto[] Videos,
    DiscogsCommunityDto? Community,
    DateTime DateAdded,
    DateTime DateChanged
);

public record DiscogsReleaseDto(
    int Id,
    string Title,
    int Year,
    string Status,
    string Uri,
    string ResourceUrl,
    string DataQuality,
    string? Released,
    string? ReleasedFormatted,
    string? Country,
    string? Notes,
    string? Thumb,
    int? MasterId,
    string? MasterUrl,
    string[] Genres,
    string[] Styles,
    DiscogsArtistDto[] Artists,
    DiscogsLabelDto[] Labels,
    DiscogsFormatDto[] Formats,
    DiscogsTrackDto[] Tracklist,
    DiscogsIdentifierDto[] Identifiers,
    DiscogsImageDto[] Images,
    DiscogsVideoDto[] Videos,
    DiscogsCommunityDto? Community,
    DateTime DateAdded,
    DateTime DateChanged
);

public record DiscogsArtistDto(
    int Id,
    string Name,
    string? Anv,
    string? Join,
    string? Role,
    string? Tracks,
    string ResourceUrl
);

public record DiscogsLabelDto(
    int Id,
    string Name,
    string Catno,
    string EntityType,
    string EntityTypeName,
    string ResourceUrl
);

public record DiscogsFormatDto(
    string Name,
    string Qty,
    string? Text,
    string[]? Descriptions
);

public record DiscogsTrackDto(
    string Position,
    string Type_,
    string Title,
    string? Duration,
    DiscogsArtistDto[]? Artists,
    DiscogsArtistDto[]? ExtraArtists,
    DiscogsSubTrackDto[]? SubTracks
);

public record DiscogsSubTrackDto(
    string Position,
    string Type_,
    string Title,
    string? Duration
);

public record DiscogsIdentifierDto(
    string Type,
    string Value,
    string? Description
);

public record DiscogsImageDto(
    string Type,
    string Uri,
    string Uri150,
    int Width,
    int Height,
    string ResourceUrl
);

public record DiscogsVideoDto(
    string Uri,
    string Title,
    string? Description,
    int? Duration,
    bool Embed
);

public record DiscogsCommunityDto(
    string Status,
    DiscogsRatingDto? Rating,
    int Want,
    int Have,
    DiscogsContributorDto[]? Contributors,
    DiscogsSubmitterDto? Submitter,
    string DataQuality
);

public record DiscogsRatingDto(
    int Count,
    decimal Average
);

public record DiscogsContributorDto(
    string Username,
    string ResourceUrl
);

public record DiscogsSubmitterDto(
    string Username,
    string ResourceUrl
);