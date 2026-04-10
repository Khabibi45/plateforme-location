using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;

public class DiscogsApiClient : IDiscogsClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DiscogsApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var apiToken = configuration["Discogs:ApiToken"]
            ?? throw new InvalidOperationException("Discogs API token is not configured. Please set 'Discogs:ApiToken' in configuration.");

        _httpClient.BaseAddress = new Uri("https://api.discogs.com/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PlateformeLocationDisques/1.0");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Discogs", $"token={apiToken}");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<DiscogsResult<DiscogsMasterReleaseDto>> GetMasterReleaseAsync(int masterId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"masters/{masterId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return DiscogsResult<DiscogsMasterReleaseDto>.NotFound(masterId);

        if (!response.IsSuccessStatusCode)
            return DiscogsResult<DiscogsMasterReleaseDto>.ApiError((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error");

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var dto = JsonSerializer.Deserialize<DiscogsMasterReleaseDto>(content, _jsonOptions);

        if (dto is null)
            return DiscogsResult<DiscogsMasterReleaseDto>.DeserializationError($"Failed to deserialize master release {masterId} from Discogs response.");

        return DiscogsResult<DiscogsMasterReleaseDto>.Success(dto);
    }

    public async Task<DiscogsResult<DiscogsReleaseDto>> GetReleaseAsync(int releaseId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"releases/{releaseId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return DiscogsResult<DiscogsReleaseDto>.NotFound(releaseId);

        if (!response.IsSuccessStatusCode)
            return DiscogsResult<DiscogsReleaseDto>.ApiError((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error");

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var dto = JsonSerializer.Deserialize<DiscogsReleaseDto>(content, _jsonOptions);

        if (dto is null)
            return DiscogsResult<DiscogsReleaseDto>.DeserializationError($"Failed to deserialize release {releaseId} from Discogs response.");

        return DiscogsResult<DiscogsReleaseDto>.Success(dto);
    }
}