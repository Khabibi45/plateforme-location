using ByteAether.Ulid;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

/// <summary>
/// Represents community-contributed data and ratings for a release.
/// </summary>
public class CommunityData
{
    public Ulid Id { get; private set; }
    public string Status { get; private set; } = "Accepted";

    // Rating
    public int RatingCount { get; private set; }
    public decimal RatingAverage { get; private set; }

    // Want/Have counts
    public int Want { get; private set; }
    public int Have { get; private set; }

    // Contributors
    public List<Contributor> Contributors { get; private set; } = [];

    // Submitter
    public string? SubmitterUsername { get; private set; }
    public string? SubmitterResourceUrl { get; private set; }

    public string DataQuality { get; private set; } = "Correct";

    private CommunityData() { }

    public static CommunityData Create(
        string status = "Accepted",
        string dataQuality = "Correct")
    {
        return new CommunityData
        {
            Id = Ulid.New(),
            Status = status,
            DataQuality = dataQuality
        };
    }

    public void SetRating(int count, decimal average)
    {
        RatingCount = count;
        RatingAverage = average;
    }

    public void SetWantHave(int want, int have)
    {
        Want = want;
        Have = have;
    }

    public void SetSubmitter(string username, string resourceUrl)
    {
        SubmitterUsername = username;
        SubmitterResourceUrl = resourceUrl;
    }

    public void SetContributors(List<Contributor> contributors)
    {
        Contributors = contributors;
    }
}

/// <summary>
/// Represents a user who contributed data to a release.
/// </summary>
public class Contributor
{
    public Ulid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string ResourceUrl { get; private set; } = string.Empty;

    private Contributor() { }

    public static Contributor Create(string username, string resourceUrl)
    {
        return new Contributor
        {
            Id = Ulid.New(),
            Username = username,
            ResourceUrl = resourceUrl
        };
    }
}
