using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a barcode, matrix number, or other identifier for a release.
/// </summary>
public class ReleaseIdentifier
{
    public Ulid Id { get; private set; }

    /// <summary>
    /// Type of identifier (e.g. "Barcode", "Matrix / Runout", "ASIN", "ISRC")
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Optional freeform description or note
    /// </summary>
    public string? Description { get; private set; }

    private ReleaseIdentifier() { }

    public static ReleaseIdentifier Create(
        string type,
        string value,
        string? description = null)
    {
        return new ReleaseIdentifier
        {
            Id = Ulid.New(),
            Type = type,
            Value = value,
            Description = description
        };
    }
}
