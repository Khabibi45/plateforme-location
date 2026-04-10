using Microsoft.AspNetCore.WebUtilities;

namespace PlateformeLocationDisques.WebApi.Shared.Hypermedia;

/// <summary>
/// Helper class to build hypermedia links for HATEOAS responses.
/// </summary>
public static class LinkBuilder
{
    /// <summary>
    /// Creates pagination links for a paginated collection.
    /// </summary>
    public static Links CreatePaginationLinks(
        string baseUrl,
        int currentPage,
        int pageSize,
        int totalPages,
        Dictionary<string, string>? additionalParams = null)
    {
        var links = new Links();

        // Self link
        links.Add("self", BuildUrl(baseUrl, currentPage, pageSize, additionalParams));

        // First page
        links.Add("first", BuildUrl(baseUrl, 1, pageSize, additionalParams));

        // Last page
        links.Add("last", BuildUrl(baseUrl, totalPages, pageSize, additionalParams));

        // Previous page
        if (currentPage > 1)
        {
            links.Add("prev", BuildUrl(baseUrl, currentPage - 1, pageSize, additionalParams));
        }
        else
        {
            links.Add("prev", null);
        }

        // Next page
        if (currentPage < totalPages)
        {
            links.Add("next", BuildUrl(baseUrl, currentPage + 1, pageSize, additionalParams));
        }
        else
        {
            links.Add("next", null);
        }

        return links;
    }

    /// <summary>
    /// Builds a URL with query parameters.
    /// </summary>
    private static string BuildUrl(
        string baseUrl,
        int page,
        int pageSize,
        Dictionary<string, string>? additionalParams = null)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        if (additionalParams != null)
        {
            foreach (var param in additionalParams)
            {
                queryParams[param.Key] = param.Value;
            }
        }

        return QueryHelpers.AddQueryString(baseUrl, queryParams);
    }

    /// <summary>
    /// Adds navigation links for browsing by genre and artist.
    /// </summary>
    public static void AddBrowseLinks(this Links links)
    {
        links.Add("genres", "/api/discogs/genres");
        links.Add("artists", "/api/discogs/artists");
        links.Add("releases", "/api/discogs/releases");
        links.Add("masterReleases", "/api/discogs/master-releases");
    }

    /// <summary>
    /// Adds a link to filter by genre.
    /// </summary>
    public static void AddGenreLink(this Links links, string genre, int page = 1, int pageSize = 20)
    {
        var url = QueryHelpers.AddQueryString($"/api/discogs/releases/genre/{Uri.EscapeDataString(genre)}",
            new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            });
        links.Add("byGenre", url);
    }

    /// <summary>
    /// Adds a link to filter by artist.
    /// </summary>
    public static void AddArtistLink(this Links links, string artist, int page = 1, int pageSize = 20)
    {
        var url = QueryHelpers.AddQueryString($"/api/discogs/releases/artist/{Uri.EscapeDataString(artist)}",
            new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            });
        links.Add("byArtist", url);
    }
}
