using CatalogoDeFilmes.Models;

namespace CatalogoDeFilmes.Services;

public interface ITmdbApiService
{
    Task<TmdbSearchResponse?> SearchMoviesAsync(string query, int page);
    Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId);
    Task<TmdbImagesResponse?> GetMovieImagesAsync(int tmdbId); // 👈 NOVO
    Task<TmdbConfiguration?> GetConfigurationAsync();
}
