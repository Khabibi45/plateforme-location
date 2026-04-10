namespace PlateformeLocationDisques.WebApi.Shared.Hypermedia;

/// <summary>
/// Collection of hypermedia links for HATEOAS responses.
/// </summary>
public class Links : Dictionary<string, Link?>
{
    /// <summary>
    /// Adds a link to the collection.
    /// </summary>
    public void Add(string rel, string? href, string method = "GET", string? type = null)
    {
        if (href == null)
        {
            this[rel] = null;
        }
        else
        {
            this[rel] = new Link(href, method, type);
        }
    }

    /// <summary>
    /// Creates a self link.
    /// </summary>
    public static Links CreateSelf(string href)
    {
        var links = new Links();
        links.Add("self", href);
        return links;
    }
}
