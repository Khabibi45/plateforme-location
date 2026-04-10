using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.GetMasterReleaseById;

/// <summary>
/// Handler for GetMasterReleaseById query.
/// </summary>
public static class GetMasterReleaseByIdHandler
{
    public static async Task<MasterReleaseDto?> Handle(
        GetMasterReleaseById query,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var masterRelease = await dbContext.MasterReleases
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

        if (masterRelease == null)
        {
            return null;
        }

        // Map domain entity to DTO (BFF pattern)
        return new MasterReleaseDto(
            Id: masterRelease.Id.ToString(),
            DiscogsId: masterRelease.DiscogsId,
            Title: masterRelease.Title,
            Year: masterRelease.Year,
            Status: masterRelease.Status,
            Genres: masterRelease.Genres.ToArray(),
            Styles: masterRelease.Styles.ToArray(),
            Artists: masterRelease.Artists.Select(a => new MasterReleaseArtistDto(
                Name: a.GetDisplayName(),
                Anv: a.Anv,
                Role: a.Role
            )).ToArray(),
            Tracklist: masterRelease.Tracklist.Select(t => new MasterReleaseTrackDto(
                Position: t.Position,
                Title: t.Title,
                Duration: t.Duration,
                Type: t.Type
            )).ToArray(),
            Thumb: masterRelease.Thumb,
            Notes: masterRelease.Notes,
            NumForSale: masterRelease.NumForSale,
            LowestPrice: masterRelease.LowestPrice,
            Community: masterRelease.Community != null
                ? new MasterReleaseCommunityDto(
                    Want: masterRelease.Community.Want,
                    Have: masterRelease.Community.Have,
                    RatingAverage: masterRelease.Community.RatingAverage,
                    RatingCount: masterRelease.Community.RatingCount
                )
                : null,
            ImportedAt: masterRelease.ImportedAt
        );
    }
}
