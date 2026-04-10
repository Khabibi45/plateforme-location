using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Features.ImportMasterRelease;

/// <summary>
/// Handler for ImportMasterRelease command.
/// Fetches a master release from Discogs API and stores it in the database.
/// </summary>
public static class ImportMasterReleaseHandler
{
    /// <summary>
    /// Wolverine automatically discovers this Handle method.
    /// </summary>
    public static async Task<MasterReleaseImported> Handle(
        ImportMasterRelease command,
        IDiscogsClient discogsClient,
        DiscogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Check if the master release already exists
        var existingMaster = await dbContext.MasterReleases
            .FirstOrDefaultAsync(m => m.DiscogsId == command.DiscogsId, cancellationToken);

        if (existingMaster != null)
        {
            return new MasterReleaseImported(
                existingMaster.Id,
                existingMaster.DiscogsId,
                existingMaster.Title,
                AlreadyExisted: true);
        }

        // Fetch from Discogs API
        var result = await discogsClient.GetMasterReleaseAsync(command.DiscogsId, cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.ErrorMessage);

        // Map DTO to domain entity
        var masterRelease = MapToEntity(result.Value!);

        // Save to database
        dbContext.MasterReleases.Add(masterRelease);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new MasterReleaseImported(
            masterRelease.Id,
            masterRelease.DiscogsId,
            masterRelease.Title,
            AlreadyExisted: false);
    }

    private static MasterRelease MapToEntity(DiscogsMasterReleaseDto dto)
    {
        var masterRelease = MasterRelease.Create(
            dto.Id,
            dto.Title,
            dto.Year,
            dto.Status,
            dto.DataQuality);

        masterRelease.UpdateMetadata(
            dto.Title,
            dto.Year,
            dto.Status,
            dto.DataQuality,
            dto.Uri,
            dto.ResourceUrl,
            dto.Thumb,
            dto.Notes);

        if (dto.MainRelease.HasValue && dto.MainReleaseUrl != null)
        {
            masterRelease.SetMainRelease(dto.MainRelease.Value, dto.MainReleaseUrl);
        }

        if (dto.MostRecentRelease.HasValue && dto.MostRecentReleaseUrl != null)
        {
            masterRelease.SetMostRecentRelease(dto.MostRecentRelease.Value, dto.MostRecentReleaseUrl);
        }

        masterRelease.SetMarketplaceData(dto.NumForSale, dto.LowestPrice);
        masterRelease.SetClassification(dto.Genres.ToList(), dto.Styles.ToList());

        // Map artists
        var artists = dto.Artists.Select(a => ReleaseArtist.Create(
            a.Id,
            a.Name,
            a.ResourceUrl,
            a.Anv,
            a.Join,
            a.Role,
            a.Tracks)).ToList();
        masterRelease.SetArtists(artists);

        // Map tracklist
        var tracks = dto.Tracklist.Select(t =>
        {
            var track = Track.Create(t.Position, t.Title, t.Type_, t.Duration);

            if (t.Artists != null && t.Artists.Length > 0)
            {
                var trackArtists = t.Artists.Select(a => ReleaseArtist.Create(
                    a.Id,
                    a.Name,
                    a.ResourceUrl,
                    a.Anv,
                    a.Join,
                    a.Role,
                    a.Tracks)).ToList();
                track.SetArtists(trackArtists);
            }

            if (t.ExtraArtists != null && t.ExtraArtists.Length > 0)
            {
                var extraArtists = t.ExtraArtists.Select(a => ReleaseArtist.Create(
                    a.Id,
                    a.Name,
                    a.ResourceUrl,
                    a.Anv,
                    null,
                    a.Role,
                    a.Tracks)).ToList();
                track.SetExtraArtists(extraArtists);
            }

            if (t.SubTracks != null && t.SubTracks.Length > 0)
            {
                var subTracks = t.SubTracks.Select(st => SubTrack.Create(
                    st.Position,
                    st.Title,
                    st.Type_,
                    st.Duration)).ToList();
                track.SetSubTracks(subTracks);
            }

            return track;
        }).ToList();
        masterRelease.SetTracklist(tracks);

        // Map images
        var images = dto.Images.Select(i => ReleaseImage.Create(
            i.Uri,
            i.Uri150,
            i.Width,
            i.Height,
            i.ResourceUrl,
            i.Type)).ToList();
        masterRelease.SetImages(images);

        // Map videos
        var videos = dto.Videos.Select(v => ReleaseVideo.Create(
            v.Uri,
            v.Title,
            v.Description,
            v.Duration,
            v.Embed)).ToList();
        masterRelease.SetVideos(videos);

        // Map community data
        if (dto.Community != null)
        {
            var community = CommunityData.Create(dto.Community.Status, dto.Community.DataQuality);

            if (dto.Community.Rating != null)
            {
                community.SetRating(dto.Community.Rating.Count, dto.Community.Rating.Average);
            }

            community.SetWantHave(dto.Community.Want, dto.Community.Have);

            if (dto.Community.Submitter != null)
            {
                community.SetSubmitter(dto.Community.Submitter.Username, dto.Community.Submitter.ResourceUrl);
            }

            if (dto.Community.Contributors != null && dto.Community.Contributors.Length > 0)
            {
                var contributors = dto.Community.Contributors.Select(c =>
                    Contributor.Create(c.Username, c.ResourceUrl)).ToList();
                community.SetContributors(contributors);
            }

            masterRelease.SetCommunity(community);
        }

        return masterRelease;
    }
}
