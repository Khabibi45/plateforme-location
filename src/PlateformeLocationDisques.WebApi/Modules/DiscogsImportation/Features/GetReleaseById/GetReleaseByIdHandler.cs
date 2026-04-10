using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetReleaseById;

/// <summary>
/// Handler for GetReleaseById query.
/// </summary>
public static class GetReleaseByIdHandler
{
    public static async Task<ReleaseDto?> Handle(
        GetReleaseById query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var release = await dbContext.Releases
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (release == null)
        {
            return null;
        }

        // Map domain entity to DTO (BFF pattern)
        return new ReleaseDto(
            Id: release.Id.ToString(),
            DiscogsId: release.DiscogsId,
            Title: release.Title,
            Year: release.Year,
            Country: release.Country,
            Status: release.Status,
            Genres: release.Genres.ToArray(),
            Styles: release.Styles.ToArray(),
            Artists: release.Artists.Select(a => new ReleaseArtistDto(
                Name: a.GetDisplayName(),
                Anv: a.Anv,
                Role: a.Role
            )).ToArray(),
            Labels: release.Labels.Select(l => new ReleaseLabelDto(
                Name: l.Name,
                CatalogNumber: l.CatalogNumber
            )).ToArray(),
            Formats: release.Formats.Select(f => new ReleaseFormatDto(
                Name: f.Name,
                Quantity: f.Quantity,
                Descriptions: f.Descriptions.ToArray()
            )).ToArray(),
            Tracklist: release.Tracklist.Select(t => new ReleaseTrackDto(
                Position: t.Position,
                Title: t.Title,
                Duration: t.Duration,
                Type: t.Type
            )).ToArray(),
            Thumb: release.Thumb,
            Notes: release.Notes,
            MasterReleaseId: release.MasterReleaseId?.ToString(),
            MasterDiscogsId: release.MasterDiscogsId,
            Community: release.Community != null
                ? new ReleaseCommunityDto(
                    Want: release.Community.Want,
                    Have: release.Community.Have,
                    RatingAverage: release.Community.RatingAverage,
                    RatingCount: release.Community.RatingCount
                )
                : null,
            ImportedAt: release.ImportedAt
        );
    }
}
