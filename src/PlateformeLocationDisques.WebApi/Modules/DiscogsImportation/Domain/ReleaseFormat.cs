using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a physical or digital format for a release
/// (e.g. Vinyl, CD, Cassette, File).
/// </summary>
public class ReleaseFormat
{
    public Ulid Id { get; private set; }

    /// <summary>
    /// Medium name (e.g. "Vinyl", "CD", "Cassette", "File")
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Quantity of this medium in the release (e.g. "1", "2")
    /// </summary>
    public string Quantity { get; private set; } = "1";

    /// <summary>
    /// Optional freeform description
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Format qualifiers (e.g. ["LP", "Album", "Stereo", "180g"])
    /// Carries rich metadata: pressing weight, speed, edition tags, packaging
    /// </summary>
    public List<string> Descriptions { get; private set; } = [];

    private ReleaseFormat() { }

    public static ReleaseFormat Create(
        string name,
        string quantity = "1",
        string? text = null,
        List<string>? descriptions = null)
    {
        return new ReleaseFormat
        {
            Id = Ulid.New(),
            Name = name,
            Quantity = quantity,
            Text = text,
            Descriptions = descriptions ?? []
        };
    }

    public string GetFormattedDescription() =>
        string.Join(", ", [Name, .. Descriptions]);
}
