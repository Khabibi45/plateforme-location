using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a linked video (YouTube or external) for a release.
/// </summary>
public class ReleaseVideo
{
    public Ulid Id { get; private set; }
    public string Uri { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int? Duration { get; private set; }

    /// <summary>
    /// Whether embedding is allowed
    /// </summary>
    public bool Embed { get; private set; }

    private ReleaseVideo() { }

    public static ReleaseVideo Create(
        string uri,
        string title,
        string? description = null,
        int? duration = null,
        bool embed = true)
    {
        return new ReleaseVideo
        {
            Id = Ulid.New(),
            Uri = uri,
            Title = title,
            Description = description,
            Duration = duration,
            Embed = embed
        };
    }
}
