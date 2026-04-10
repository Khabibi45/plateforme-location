using ByteAether.Ulid;
using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

/// <summary>
/// Seeds the database with realistic test data for local development.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(DiscogsDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting database seeding...");

        // Check if already seeded
        if (await context.MasterReleases.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        // Create a realistic master release: Pink Floyd - The Dark Side of the Moon
        var masterRelease = MasterRelease.Create(
            discogsId: 1000,
            title: "The Dark Side Of The Moon",
            year: 1973,
            status: "Accepted",
            dataQuality: "Correct"
        );

        // Add genres and styles
        masterRelease.Genres.Add("Rock");
        masterRelease.Styles.Add("Psychedelic Rock");
        masterRelease.Styles.Add("Progressive Rock");

        // Add artists
        masterRelease.Artists.Add(ReleaseArtist.Create(
            discogsArtistId: 10001,
            name: "Pink Floyd",
            resourceUrl: "https://api.discogs.com/artists/10001"
        ));

        // Add tracklist (all 10 tracks from the album)
        var tracks = new[]
        {
            ("Speak To Me", "1", "1:13"),
            ("Breathe", "2", "2:43"),
            ("On The Run", "3", "3:36"),
            ("Time", "4", "6:53"),
            ("The Great Gig In The Sky", "5", "4:36"),
            ("Money", "6", "6:23"),
            ("Us And Them", "7", "7:49"),
            ("Any Colour You Like", "8", "3:26"),
            ("Brain Damage", "9", "3:49"),
            ("Eclipse", "10", "2:03")
        };

        foreach (var (title, position, duration) in tracks)
        {
            masterRelease.Tracklist.Add(Track.Create(
                position: position,
                title: title,
                duration: duration
            ));
        }

        // Add community data
        var masterCommunity = CommunityData.Create();
        masterCommunity.SetWantHave(8234, 12543);
        masterCommunity.SetRating(1842, 4.67m);
        masterRelease.SetCommunity(masterCommunity);

        // Add images
        masterRelease.Images.Add(ReleaseImage.Create(
            uri: "https://i.discogs.com/example/dark-side-moon.jpg",
            uri150: "https://i.discogs.com/example/dark-side-moon-150.jpg",
            width: 600,
            height: 600,
            resourceUrl: "https://api.discogs.com/images/example",
            type: "primary"
        ));

        context.MasterReleases.Add(masterRelease);

        // Create a specific release (pressing) of this master
        var release = Release.Create(
            discogsId: 2000,
            title: "The Dark Side Of The Moon",
            year: 1973,
            status: "Accepted",
            dataQuality: "Correct"
        );
        release.UpdateMetadata(
            title: "The Dark Side Of The Moon",
            year: 1973,
            status: "Accepted",
            dataQuality: "Correct",
            uri: "https://www.discogs.com/release/2000",
            resourceUrl: "https://api.discogs.com/releases/2000",
            country: "UK",
            thumb: "https://i.discogs.com/example/dark-side-moon-uk-150.jpg"
        );
        release.LinkToMaster(1000, "https://api.discogs.com/masters/1000", masterRelease.Id);

        // Add genres and styles
        release.Genres.Add("Rock");
        release.Styles.Add("Psychedelic Rock");
        release.Styles.Add("Progressive Rock");

        // Add artists (same as master)
        release.Artists.Add(ReleaseArtist.Create(
            discogsArtistId: 10001,
            name: "Pink Floyd",
            resourceUrl: "https://api.discogs.com/artists/10001"
        ));

        // Add labels
        release.Labels.Add(ReleaseLabel.Create(
            discogsLabelId: 5001,
            name: "Harvest",
            catalogNumber: "SHVL 804",
            resourceUrl: "https://api.discogs.com/labels/5001"
        ));

        // Add formats
        release.Formats.Add(ReleaseFormat.Create(
            name: "Vinyl",
            quantity: "1",
            text: "Gatefold",
            descriptions: ["LP", "Album", "Gatefold"]
        ));

        // Add identifiers
        release.Identifiers.Add(ReleaseIdentifier.Create(
            type: "Matrix / Runout",
            value: "SHVL 804 A-2 // 1",
            description: "Side A runout"
        ));

        release.Identifiers.Add(ReleaseIdentifier.Create(
            type: "Matrix / Runout",
            value: "SHVL 804 B-2 // 1",
            description: "Side B runout"
        ));

        // Add tracklist (same as master)
        foreach (var (title, position, duration) in tracks)
        {
            release.Tracklist.Add(Track.Create(
                position: position,
                title: title,
                duration: duration
            ));
        }

        // Add community data
        var releaseCommunity = CommunityData.Create();
        releaseCommunity.SetWantHave(892, 3421);
        releaseCommunity.SetRating(542, 4.72m);
        release.SetCommunity(releaseCommunity);

        // Add images
        release.Images.Add(ReleaseImage.Create(
            uri: "https://i.discogs.com/example/dark-side-moon-uk.jpg",
            uri150: "https://i.discogs.com/example/dark-side-moon-uk-150.jpg",
            width: 600,
            height: 600,
            resourceUrl: "https://api.discogs.com/images/example-uk",
            type: "primary"
        ));

        // Note: Thumb is already set in UpdateMetadata above

        context.Releases.Add(release);

        // Save all changes
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeding completed successfully. Added 1 master release and 1 release.");
    }
}
