using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents a canonical, format-agnostic entry for a recording.
/// Acts as a parent grouping all physical/digital versions (Releases).
/// </summary>
public class MasterRelease
{
    public Ulid Id { get; private set; }

    // Identity
    public int DiscogsId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Uri { get; private set; } = string.Empty;
    public string ResourceUrl { get; private set; } = string.Empty;
    public string DataQuality { get; private set; } = string.Empty;

    // Master Release Info
    public int Year { get; private set; }
    public int? MainReleaseId { get; private set; }
    public string? MainReleaseUrl { get; private set; }
    public int? MostRecentReleaseId { get; private set; }
    public string? MostRecentReleaseUrl { get; private set; }
    public string? VersionsUrl { get; private set; }
    public int? NumForSale { get; private set; }
    public decimal? LowestPrice { get; private set; }
    public string? Thumb { get; private set; }
    public string? Notes { get; private set; }

    // Classification
    public List<string> Genres { get; private set; } = [];
    public List<string> Styles { get; private set; } = [];

    // Artists
    public List<ReleaseArtist> Artists { get; private set; } = [];

    // Tracklist
    public List<Track> Tracklist { get; private set; } = [];

    // Media
    public List<ReleaseImage> Images { get; private set; } = [];
    public List<ReleaseVideo> Videos { get; private set; } = [];

    // Community Data
    public CommunityData? Community { get; private set; }

    // Timestamps
    public DateTime DateAdded { get; private set; }
    public DateTime DateChanged { get; private set; }
    public DateTime ImportedAt { get; private set; }

    // Navigation properties
    public List<Release> Releases { get; private set; } = [];

    private MasterRelease() { }

    public static MasterRelease Create(
        int discogsId,
        string title,
        int year,
        string status = "Accepted",
        string dataQuality = "Correct")
    {
        return new MasterRelease
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
        string? thumb = null,
        string? notes = null)
    {
        Title = title;
        Year = year;
        Status = status;
        DataQuality = dataQuality;
        Uri = uri;
        ResourceUrl = resourceUrl;
        Thumb = thumb;
        Notes = notes;
        DateChanged = DateTime.UtcNow;
    }

    public void SetMainRelease(int mainReleaseId, string mainReleaseUrl)
    {
        MainReleaseId = mainReleaseId;
        MainReleaseUrl = mainReleaseUrl;
    }

    public void SetMostRecentRelease(int mostRecentReleaseId, string mostRecentReleaseUrl)
    {
        MostRecentReleaseId = mostRecentReleaseId;
        MostRecentReleaseUrl = mostRecentReleaseUrl;
    }

    public void SetMarketplaceData(int? numForSale, decimal? lowestPrice)
    {
        NumForSale = numForSale;
        LowestPrice = lowestPrice;
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

    public void SetTracklist(List<Track> tracklist)
    {
        Tracklist = tracklist;
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
