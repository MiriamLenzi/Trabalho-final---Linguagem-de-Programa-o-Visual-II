using System.Net.Http.Json;
using System.Text.Json;
using CatalogoDeFilmes.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CatalogoDeFilmes.Services;

public class TmdbApiService : ITmdbApiService
{
    private readonly HttpClient _httpClient;
    private readonly TmdbOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TmdbApiService> _logger;

    private const string SearchCachePrefix = "tmdb:search:";
    private const string DetailsCachePrefix = "tmdb:details:";
    private const string ImagesCachePrefix = "tmdb:images:";   // 👈 NOVO
    private const string ConfigCacheKey = "tmdb:config";

    public TmdbApiService(
        HttpClient httpClient,
        IOptions<TmdbOptions> options,
        IMemoryCache cache,
        ILogger<TmdbApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;

        if (_options.UseBearerToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.BearerTokenV4);
        }
    }

    public async Task<TmdbSearchResponse?> SearchMoviesAsync(string query, int page)
    {
        var cacheKey = $"{SearchCachePrefix}{query}:{page}";
        if (_cache.TryGetValue(cacheKey, out TmdbSearchResponse? cached))
            return cached;

        var url = $"{_options.BaseUrl}/search/movie?query={Uri.EscapeDataString(query)}&page={page}&language=pt-BR";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!_options.UseBearerToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_options.BearerTokenV4}");
            // ou &api_key= se preferir v3
        }

        var start = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);

        _logger.LogInformation("TMDb SEARCH: {Url} - Status {Status} - {Date}",
            url, response.StatusCode, start);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro TMDb Search. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
        if (result != null)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5)); // RF08
        }

        return result;
    }

    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId)
    {
        var cacheKey = $"{DetailsCachePrefix}{tmdbId}";
        if (_cache.TryGetValue(cacheKey, out TmdbMovieDetails? cached))
            return cached;

        var url = $"{_options.BaseUrl}/movie/{tmdbId}?language=pt-BR&append_to_response=credits";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!_options.UseBearerToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_options.BearerTokenV4}");
        }

        var start = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);

        _logger.LogInformation("TMDb DETAILS: {Url} - Status {Status} - {Date}",
            url, response.StatusCode, start);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro TMDb Details. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TmdbMovieDetails>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result != null)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    // 👇 NOVO – /movie/{id}/images com cache e logs
    public async Task<TmdbImagesResponse?> GetMovieImagesAsync(int tmdbId)
    {
        var cacheKey = $"{ImagesCachePrefix}{tmdbId}";
        if (_cache.TryGetValue(cacheKey, out TmdbImagesResponse? cached))
            return cached;

        var url = $"{_options.BaseUrl}/movie/{tmdbId}/images";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!_options.UseBearerToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_options.BearerTokenV4}");
        }

        var start = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);

        _logger.LogInformation("TMDb IMAGES: {Url} - Status {Status} - {Date}",
            url, response.StatusCode, start);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro TMDb Images. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TmdbImagesResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result != null)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    public async Task<TmdbConfiguration?> GetConfigurationAsync()
    {
        if (_cache.TryGetValue(ConfigCacheKey, out TmdbConfiguration? cached))
            return cached;

        var url = $"{_options.BaseUrl}/configuration";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!_options.UseBearerToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_options.BearerTokenV4}");
        }

        var start = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);

        _logger.LogInformation("TMDb CONFIG: {Url} - Status {Status} - {Date}",
            url, response.StatusCode, start);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro TMDb Config. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TmdbConfiguration>();
        if (result != null)
        {
            _cache.Set(ConfigCacheKey, result, TimeSpan.FromHours(6));
        }

        return result;
    }
}
