using CatalogoDeFilmes.Models;

namespace CatalogoDeFilmes.Services;

public interface IWeatherApiService
{
    Task<WeatherForecastResponse?> GetDailyForecastAsync(double latitude, double longitude);
}
