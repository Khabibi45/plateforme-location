using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a label that released a specific release.
/// </summary>
public class ReleaseLabel
{
    public Ulid Id { get; private set; }
    public int DiscogsLabelId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Catalog number assigned by the label for this release
    /// </summary>
    public string CatalogNumber { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;
    public string EntityTypeName { get; private set; } = string.Empty;
    public string ResourceUrl { get; private set; } = string.Empty;

    private ReleaseLabel() { }

    public static ReleaseLabel Create(
        int discogsLabelId,
        string name,
        string catalogNumber,
        string resourceUrl,
        string entityType = "1",
        string entityTypeName = "Label")
    {
        return new ReleaseLabel
        {
            Id = Ulid.New(),
            DiscogsLabelId = discogsLabelId,
            Name = name,
            CatalogNumber = catalogNumber,
            ResourceUrl = resourceUrl,
            EntityType = entityType,
            EntityTypeName = entityTypeName
        };
    }
}
