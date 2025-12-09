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
    private const string ImagesCachePrefix = "tmdb:images:";
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

        // Configurar autenticação apenas uma vez no construtor
        if (_options.UseBearerToken && !string.IsNullOrEmpty(_options.BearerTokenV4))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.BearerTokenV4);
        }
    }

    public async Task<TmdbSearchResponse?> SearchMoviesAsync(string query, int page)
    {
        var cacheKey = $"{SearchCachePrefix}{query}:{page}";
        if (_cache.TryGetValue(cacheKey, out TmdbSearchResponse? cached))
        {
            _logger.LogInformation("TMDb SEARCH (CACHE): {Query} - Page {Page}", query, page);
            return cached;
        }

        var url = _options.UseBearerToken
            ? $"{_options.BaseUrl}/search/movie?query={Uri.EscapeDataString(query)}&page={page}&language=pt-BR"
            : $"{_options.BaseUrl}/search/movie?api_key={_options.ApiKeyV3}&query={Uri.EscapeDataString(query)}&page={page}&language=pt-BR";

        var start = DateTime.UtcNow;
        
        try
        {
            var response = await _httpClient.GetAsync(url);

            _logger.LogInformation("TMDb SEARCH: {Query} - Page {Page} - Status {Status} - Duration {Duration}ms",
                query, page, response.StatusCode, (DateTime.UtcNow - start).TotalMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro TMDb Search. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar filmes no TMDb: {Query}", query);
            return null;
        }
    }

    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId)
    {
        var cacheKey = $"{DetailsCachePrefix}{tmdbId}";
        if (_cache.TryGetValue(cacheKey, out TmdbMovieDetails? cached))
        {
            _logger.LogInformation("TMDb DETAILS (CACHE): {TmdbId}", tmdbId);
            return cached;
        }

        var url = _options.UseBearerToken
            ? $"{_options.BaseUrl}/movie/{tmdbId}?language=pt-BR&append_to_response=credits"
            : $"{_options.BaseUrl}/movie/{tmdbId}?api_key={_options.ApiKeyV3}&language=pt-BR&append_to_response=credits";

        var start = DateTime.UtcNow;
        
        try
        {
            var response = await _httpClient.GetAsync(url);

            _logger.LogInformation("TMDb DETAILS: {TmdbId} - Status {Status} - Duration {Duration}ms",
                tmdbId, response.StatusCode, (DateTime.UtcNow - start).TotalMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro TMDb Details. TmdbId: {TmdbId}, Status: {Status}. Corpo: {Body}", 
                    tmdbId, response.StatusCode, body);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar detalhes do filme no TMDb: {TmdbId}", tmdbId);
            return null;
        }
    }

    public async Task<TmdbImagesResponse?> GetMovieImagesAsync(int tmdbId)
    {
        var cacheKey = $"{ImagesCachePrefix}{tmdbId}";
        if (_cache.TryGetValue(cacheKey, out TmdbImagesResponse? cached))
        {
            _logger.LogInformation("TMDb IMAGES (CACHE): {TmdbId}", tmdbId);
            return cached;
        }

        var url = _options.UseBearerToken
            ? $"{_options.BaseUrl}/movie/{tmdbId}/images"
            : $"{_options.BaseUrl}/movie/{tmdbId}/images?api_key={_options.ApiKeyV3}";

        var start = DateTime.UtcNow;
        
        try
        {
            var response = await _httpClient.GetAsync(url);

            _logger.LogInformation("TMDb IMAGES: {TmdbId} - Status {Status} - Duration {Duration}ms",
                tmdbId, response.StatusCode, (DateTime.UtcNow - start).TotalMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro TMDb Images. TmdbId: {TmdbId}, Status: {Status}. Corpo: {Body}", 
                    tmdbId, response.StatusCode, body);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar imagens do filme no TMDb: {TmdbId}", tmdbId);
            return null;
        }
    }

    public async Task<TmdbConfiguration?> GetConfigurationAsync()
    {
        if (_cache.TryGetValue(ConfigCacheKey, out TmdbConfiguration? cached))
        {
            _logger.LogInformation("TMDb CONFIG (CACHE)");
            return cached;
        }

        var url = _options.UseBearerToken
            ? $"{_options.BaseUrl}/configuration"
            : $"{_options.BaseUrl}/configuration?api_key={_options.ApiKeyV3}";

        var start = DateTime.UtcNow;
        
        try
        {
            var response = await _httpClient.GetAsync(url);

            _logger.LogInformation("TMDb CONFIG - Status {Status} - Duration {Duration}ms",
                response.StatusCode, (DateTime.UtcNow - start).TotalMilliseconds);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao buscar configuração do TMDb");
            return null;
        }
    }
}