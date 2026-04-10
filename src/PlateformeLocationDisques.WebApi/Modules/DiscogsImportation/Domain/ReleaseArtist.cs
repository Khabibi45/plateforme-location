using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents an artist credited on a release or track.
/// </summary>
public class ReleaseArtist
{
    public Ulid Id { get; private set; }
    public int DiscogsArtistId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Artist Name Variation - how the artist is credited on this specific release
    /// </summary>
    public string? Anv { get; private set; }

    /// <summary>
    /// Join string between artists (e.g. "&", ",", "Featuring")
    /// </summary>
    public string? Join { get; private set; }

    /// <summary>
    /// Role of the artist (e.g. "Producer", "Remixed By")
    /// </summary>
    public string? Role { get; private set; }

    /// <summary>
    /// Track positions where this artist appears (for track-specific credits)
    /// </summary>
    public string? Tracks { get; private set; }

    public string ResourceUrl { get; private set; } = string.Empty;

    private ReleaseArtist() { }

    public static ReleaseArtist Create(
        int discogsArtistId,
        string name,
        string resourceUrl,
        string? anv = null,
        string? join = null,
        string? role = null,
        string? tracks = null)
    {
        return new ReleaseArtist
        {
            Id = Ulid.New(),
            DiscogsArtistId = discogsArtistId,
            Name = name,
            ResourceUrl = resourceUrl,
            Anv = anv,
            Join = join,
            Role = role,
            Tracks = tracks
        };
    }

    /// <summary>
    /// Gets the display name - uses ANV if available, otherwise canonical name
    /// </summary>
    public string GetDisplayName() => !string.IsNullOrWhiteSpace(Anv) ? Anv : Name;
}
