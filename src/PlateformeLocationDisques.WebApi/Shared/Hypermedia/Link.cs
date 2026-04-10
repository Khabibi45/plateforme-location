namespace PlateformeLocationDisques.WebApi.Shared.Hypermedia;

/// <summary>
/// Represents a hypermedia link in a HATEOAS response.
/// </summary>
/// <param name="Href">The URL of the linked resource.</param>
/// <param name="Method">The HTTP method to use (default: GET).</param>
/// <param name="Type">The media type of the linked resource.</param>
public record Link(
    string Href,
    string Method = "GET",
    string? Type = null
);
