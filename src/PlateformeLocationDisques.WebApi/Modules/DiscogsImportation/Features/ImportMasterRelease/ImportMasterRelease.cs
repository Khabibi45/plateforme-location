using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.ImportMasterRelease;

/// <summary>
/// Command to import a master release from Discogs by its ID.
/// </summary>
public record ImportMasterRelease(int DiscogsId);

/// <summary>
/// Result of importing a master release.
/// </summary>
public record MasterReleaseImported(Ulid Id, int DiscogsId, string Title, bool AlreadyExisted);
