using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents cover art or media scans for a release.
/// </summary>
public class ReleaseImage
{
    public Ulid Id { get; private set; }

    /// <summary>
    /// Image type: "primary" or "secondary"
    /// </summary>
    public string Type { get; private set; } = "primary";

    /// <summary>
    /// Full-size image URL (requires auth)
    /// </summary>
    public string Uri { get; private set; } = string.Empty;

    /// <summary>
    /// Thumbnail URL 150×150 (requires auth)
    /// </summary>
    public string Uri150 { get; private set; } = string.Empty;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public string ResourceUrl { get; private set; } = string.Empty;

    private ReleaseImage() { }

    public static ReleaseImage Create(
        string uri,
        string uri150,
        int width,
        int height,
        string resourceUrl,
        string type = "primary")
    {
        return new ReleaseImage
        {
            Id = Ulid.New(),
            Type = type,
            Uri = uri,
            Uri150 = uri150,
            Width = width,
            Height = height,
            ResourceUrl = resourceUrl
        };
    }

    public bool IsPrimary() => Type == "primary";
}
