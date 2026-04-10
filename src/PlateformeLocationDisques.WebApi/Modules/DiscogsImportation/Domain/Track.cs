using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a track on a release.
/// </summary>
public class Track
{
    public Ulid Id { get; private set; }

    /// <summary>
    /// Track position (e.g. "A1", "B2", "1", "2-3")
    /// </summary>
    public string Position { get; private set; } = string.Empty;

    /// <summary>
    /// Track type: "track", "index", "heading"
    /// </summary>
    public string Type { get; private set; } = "track";

    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Duration formatted as "M:SS"
    /// </summary>
    public string? Duration { get; private set; }

    /// <summary>
    /// Per-track artist override (when different from release artists)
    /// </summary>
    public List<ReleaseArtist> Artists { get; private set; } = [];

    /// <summary>
    /// Per-track additional credits (producers, remixers, etc.)
    /// </summary>
    public List<ReleaseArtist> ExtraArtists { get; private set; } = [];

    /// <summary>
    /// Sub-tracks (for medleys or index tracks)
    /// </summary>
    public List<SubTrack> SubTracks { get; private set; } = [];

    private Track() { }

    public static Track Create(
        string position,
        string title,
        string type = "track",
        string? duration = null)
    {
        return new Track
        {
            Id = Ulid.New(),
            Position = position,
            Title = title,
            Type = type,
            Duration = duration
        };
    }

    public void SetArtists(List<ReleaseArtist> artists)
    {
        Artists = artists;
    }

    public void SetExtraArtists(List<ReleaseArtist> extraArtists)
    {
        ExtraArtists = extraArtists;
    }

    public void SetSubTracks(List<SubTrack> subTracks)
    {
        SubTracks = subTracks;
    }

    public bool IsHeading() => Type == "heading";
    public bool IsIndexTrack() => Type == "index";
}

/// <summary>
/// Represents a sub-track within a track (for medleys or index tracks).
/// </summary>
public class SubTrack
{
    public Ulid Id { get; private set; }
    public string Position { get; private set; } = string.Empty;
    public string Type { get; private set; } = "track";
    public string Title { get; private set; } = string.Empty;
    public string? Duration { get; private set; }

    private SubTrack() { }

    public static SubTrack Create(
        string position,
        string title,
        string type = "track",
        string? duration = null)
    {
        return new SubTrack
        {
            Id = Ulid.New(),
            Position = position,
            Title = title,
            Type = type,
            Duration = duration
        };
    }
}
