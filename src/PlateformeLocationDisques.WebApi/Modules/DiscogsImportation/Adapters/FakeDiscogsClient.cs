namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;

public class FakeDiscogsClient : IDiscogsClient
{
    private readonly Dictionary<int, DiscogsMasterReleaseDto> _masterReleases = new();
    private readonly Dictionary<int, DiscogsReleaseDto> _releases = new();

    public FakeDiscogsClient()
    {
        SeedTestData();
    }

    public Task<DiscogsResult<DiscogsMasterReleaseDto>> GetMasterReleaseAsync(int masterId, CancellationToken cancellationToken = default)
    {
        if (_masterReleases.TryGetValue(masterId, out var masterRelease))
            return Task.FromResult(DiscogsResult<DiscogsMasterReleaseDto>.Success(masterRelease));

        return Task.FromResult(DiscogsResult<DiscogsMasterReleaseDto>.NotFound(masterId));
    }

    public Task<DiscogsResult<DiscogsReleaseDto>> GetReleaseAsync(int releaseId, CancellationToken cancellationToken = default)
    {
        if (_releases.TryGetValue(releaseId, out var release))
            return Task.FromResult(DiscogsResult<DiscogsReleaseDto>.Success(release));

        return Task.FromResult(DiscogsResult<DiscogsReleaseDto>.NotFound(releaseId));
    }

    public void AddFakeMasterRelease(DiscogsMasterReleaseDto masterRelease)
    {
        _masterReleases[masterRelease.Id] = masterRelease;
    }

    public void AddFakeRelease(DiscogsReleaseDto release)
    {
        _releases[release.Id] = release;
    }

    private void SeedTestData()
    {
        var darkSideMaster = new DiscogsMasterReleaseDto(
            Id: 1,
            Title: "The Dark Side Of The Moon",
            Year: 1973,
            Status: "Accepted",
            Uri: "/master/1",
            ResourceUrl: "https://api.discogs.com/masters/1",
            DataQuality: "Correct",
            MainRelease: 1000,
            MainReleaseUrl: "https://api.discogs.com/releases/1000",
            MostRecentRelease: 2000,
            MostRecentReleaseUrl: "https://api.discogs.com/releases/2000",
            VersionsUrl: "https://api.discogs.com/masters/1/versions",
            NumForSale: 150,
            LowestPrice: 15.99m,
            Thumb: "https://i.discogs.com/150x150.jpg",
            Notes: "One of the best-selling albums of all time",
            Genres: ["Rock"],
            Styles: ["Progressive Rock", "Psychedelic Rock"],
            Artists:
            [
                new DiscogsArtistDto(
                    Id: 45467,
                    Name: "Pink Floyd",
                    Anv: null,
                    Join: null,
                    Role: null,
                    Tracks: null,
                    ResourceUrl: "https://api.discogs.com/artists/45467"
                )
            ],
            Tracklist:
            [
                new DiscogsTrackDto("1", "track", "Speak To Me", "1:08", null, null, null),
                new DiscogsTrackDto("2", "track", "Breathe", "2:49", null, null, null),
                new DiscogsTrackDto("3", "track", "On The Run", "3:33", null, null, null),
                new DiscogsTrackDto("4", "track", "Time", "7:06", null, null, null),
                new DiscogsTrackDto("5", "track", "The Great Gig In The Sky", "4:48", null, null, null),
                new DiscogsTrackDto("6", "track", "Money", "6:30", null, null, null),
                new DiscogsTrackDto("7", "track", "Us And Them", "7:51", null, null, null),
                new DiscogsTrackDto("8", "track", "Any Colour You Like", "3:24", null, null, null),
                new DiscogsTrackDto("9", "track", "Brain Damage", "3:50", null, null, null),
                new DiscogsTrackDto("10", "track", "Eclipse", "2:09", null, null, null)
            ],
            Images:
            [
                new DiscogsImageDto(
                    Type: "primary",
                    Uri: "https://i.discogs.com/full.jpg",
                    Uri150: "https://i.discogs.com/150x150.jpg",
                    Width: 600,
                    Height: 600,
                    ResourceUrl: "https://api.discogs.com/images/1"
                )
            ],
            Videos: [],
            Community: new DiscogsCommunityDto(
                Status: "Accepted",
                Rating: new DiscogsRatingDto(Count: 5000, Average: 4.85m),
                Want: 10000,
                Have: 25000,
                Contributors: null,
                Submitter: new DiscogsSubmitterDto("discogsuser", "https://api.discogs.com/users/discogsuser"),
                DataQuality: "Correct"
            ),
            DateAdded: new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateChanged: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _masterReleases[1] = darkSideMaster;

        var darkSideRelease = new DiscogsReleaseDto(
            Id: 1000,
            Title: "The Dark Side Of The Moon",
            Year: 1973,
            Status: "Accepted",
            Uri: "/release/1000",
            ResourceUrl: "https://api.discogs.com/releases/1000",
            DataQuality: "Correct",
            Released: "1973-03-01",
            ReleasedFormatted: "01 Mar 1973",
            Country: "UK",
            Notes: "Original UK pressing",
            Thumb: "https://i.discogs.com/150x150.jpg",
            MasterId: 1,
            MasterUrl: "https://api.discogs.com/masters/1",
            Genres: ["Rock"],
            Styles: ["Progressive Rock", "Psychedelic Rock"],
            Artists:
            [
                new DiscogsArtistDto(
                    Id: 45467,
                    Name: "Pink Floyd",
                    Anv: null,
                    Join: null,
                    Role: null,
                    Tracks: null,
                    ResourceUrl: "https://api.discogs.com/artists/45467"
                )
            ],
            Labels:
            [
                new DiscogsLabelDto(
                    Id: 895,
                    Name: "Harvest",
                    Catno: "SHVL 804",
                    EntityType: "1",
                    EntityTypeName: "Label",
                    ResourceUrl: "https://api.discogs.com/labels/895"
                )
            ],
            Formats:
            [
                new DiscogsFormatDto(
                    Name: "Vinyl",
                    Qty: "1",
                    Text: null,
                    Descriptions: ["LP", "Album", "Gatefold"]
                )
            ],
            Tracklist:
            [
                new DiscogsTrackDto("A1", "track", "Speak To Me", "1:08", null, null, null),
                new DiscogsTrackDto("A2", "track", "Breathe", "2:49", null, null, null),
                new DiscogsTrackDto("A3", "track", "On The Run", "3:33", null, null, null),
                new DiscogsTrackDto("A4", "track", "Time", "7:06", null, null, null),
                new DiscogsTrackDto("A5", "track", "The Great Gig In The Sky", "4:48", null, null, null),
                new DiscogsTrackDto("B1", "track", "Money", "6:30", null, null, null),
                new DiscogsTrackDto("B2", "track", "Us And Them", "7:51", null, null, null),
                new DiscogsTrackDto("B3", "track", "Any Colour You Like", "3:24", null, null, null),
                new DiscogsTrackDto("B4", "track", "Brain Damage", "3:50", null, null, null),
                new DiscogsTrackDto("B5", "track", "Eclipse", "2:09", null, null, null)
            ],
            Identifiers:
            [
                new DiscogsIdentifierDto(Type: "Barcode", Value: "5099902894713", Description: null),
                new DiscogsIdentifierDto(Type: "Matrix / Runout", Value: "SHVL 804 A-1", Description: "Side A")
            ],
            Images:
            [
                new DiscogsImageDto(
                    Type: "primary",
                    Uri: "https://i.discogs.com/full.jpg",
                    Uri150: "https://i.discogs.com/150x150.jpg",
                    Width: 600,
                    Height: 600,
                    ResourceUrl: "https://api.discogs.com/images/1"
                )
            ],
            Videos: [],
            Community: new DiscogsCommunityDto(
                Status: "Accepted",
                Rating: new DiscogsRatingDto(Count: 2500, Average: 4.90m),
                Want: 5000,
                Have: 15000,
                Contributors: null,
                Submitter: new DiscogsSubmitterDto("discogsuser", "https://api.discogs.com/users/discogsuser"),
                DataQuality: "Correct"
            ),
            DateAdded: new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateChanged: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        _releases[1000] = darkSideRelease;
    }
}
