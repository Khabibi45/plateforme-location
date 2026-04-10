using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a specific pressing or edition of a recording
/// (e.g. US vinyl 1991, UK CD 1992).
/// </summary>
public class Release
{
    public Ulid Id { get; private set; }

    // Identity
    public int DiscogsId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Uri { get; private set; } = string.Empty;
    public string ResourceUrl { get; private set; } = string.Empty;
    public string DataQuality { get; private set; } = string.Empty;

    // Release Info
    public int Year { get; private set; }
    public string? Released { get; private set; }
    public string? ReleasedFormatted { get; private set; }
    public string? Country { get; private set; }
    public string? Notes { get; private set; }
    public string? Thumb { get; private set; }

    // Master Link
    public int? MasterDiscogsId { get; private set; }
    public string? MasterUrl { get; private set; }
    public Ulid? MasterReleaseId { get; private set; }

    // Classification
    public List<string> Genres { get; private set; } = [];
    public List<string> Styles { get; private set; } = [];

    // Artists
    public List<ReleaseArtist> Artists { get; private set; } = [];

    // Labels
    public List<ReleaseLabel> Labels { get; private set; } = [];

    // Formats
    public List<ReleaseFormat> Formats { get; private set; } = [];

    // Tracklist
    public List<Track> Tracklist { get; private set; } = [];

    // Identifiers (Barcodes, Matrix Numbers, etc.)
    public List<ReleaseIdentifier> Identifiers { get; private set; } = [];

    // Media
    public List<ReleaseImage> Images { get; private set; } = [];
    public List<ReleaseVideo> Videos { get; private set; } = [];

    // Community Data
    public CommunityData? Community { get; private set; }

    // Timestamps
    public DateTime DateAdded { get; private set; }
    public DateTime DateChanged { get; private set; }
    public DateTime ImportedAt { get; private set; }

    // Navigation
    public MasterRelease? MasterRelease { get; private set; }

    private Release() { }

    public static Release Create(
        int discogsId,
        string title,
        int year,
        string status = "Accepted",
        string dataQuality = "Correct")
    {
        return new Release
        {
            Id = Ulid.New(),
            DiscogsId = discogsId,
            Title = title,
            Year = year,
            Status = status,
            DataQuality = dataQuality,
            ImportedAt = DateTime.UtcNow,
            DateAdded = DateTime.UtcNow,
            DateChanged = DateTime.UtcNow
        };
    }

    public void UpdateMetadata(
        string title,
        int year,
        string status,
        string dataQuality,
        string uri,
        string resourceUrl,
        string? country = null,
        string? released = null,
        string? releasedFormatted = null,
        string? thumb = null,
        string? notes = null)
    {
        Title = title;
        Year = year;
        Status = status;
        DataQuality = dataQuality;
        Uri = uri;
        ResourceUrl = resourceUrl;
        Country = country;
        Released = released;
        ReleasedFormatted = releasedFormatted;
        Thumb = thumb;
        Notes = notes;
        DateChanged = DateTime.UtcNow;
    }

    public void LinkToMaster(int masterDiscogsId, string masterUrl, Ulid? masterReleaseId = null)
    {
        MasterDiscogsId = masterDiscogsId;
        MasterUrl = masterUrl;
        MasterReleaseId = masterReleaseId;
    }

    public void SetClassification(List<string> genres, List<string> styles)
    {
        Genres = genres;
        Styles = styles;
    }

    public void SetArtists(List<ReleaseArtist> artists)
    {
        Artists = artists;
    }

    public void SetLabels(List<ReleaseLabel> labels)
    {
        Labels = labels;
    }

    public void SetFormats(List<ReleaseFormat> formats)
    {
        Formats = formats;
    }

    public void SetTracklist(List<Track> tracklist)
    {
        Tracklist = tracklist;
    }

    public void SetIdentifiers(List<ReleaseIdentifier> identifiers)
    {
        Identifiers = identifiers;
    }

    public void SetImages(List<ReleaseImage> images)
    {
        Images = images;
    }

    public void SetVideos(List<ReleaseVideo> videos)
    {
        Videos = videos;
    }

    public void SetCommunity(CommunityData community)
    {
        Community = community;
    }
}
