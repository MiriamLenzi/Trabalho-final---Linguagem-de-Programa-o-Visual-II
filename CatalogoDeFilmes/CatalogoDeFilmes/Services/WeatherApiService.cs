using System.Text.Json;
using CatalogoDeFilmes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CatalogoDeFilmes.Services;

public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherApiService> _logger;

    private const string CachePrefix = "weather:";

    public WeatherApiService(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherApiService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<WeatherForecastResponse?> GetDailyForecastAsync(double latitude, double longitude)
    {
        var cacheKey = $"{CachePrefix}{latitude:F4}:{longitude:F4}";
        if (_cache.TryGetValue(cacheKey, out WeatherForecastResponse? cached))
            return cached;

        var url = $"https://api.open-meteo.com/v1/forecast" +
                  $"?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  $"&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  $"&daily=temperature_2m_max,temperature_2m_min&timezone=auto";

        var start = DateTime.UtcNow;
        var response = await _httpClient.GetAsync(url);

        _logger.LogInformation("OPEN-METEO: {Url} - Status {Status} - {Date}",
            url, response.StatusCode, start);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro Open-Meteo. Status: {Status}. Corpo: {Body}", response.StatusCode, body);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WeatherForecastResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result != null)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }
}
