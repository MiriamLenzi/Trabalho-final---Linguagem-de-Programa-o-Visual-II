using CatalogoDeFilmes.Models;

namespace CatalogoDeFilmes.Repositories;

public interface IFilmeRepository
{
    Task<List<Filme>> ListAsync();
    Task<Filme?> GetByIdAsync(int id);
    Task<Filme?> GetByTmdbIdAsync(int tmdbId);
    Task CreateAsync(Filme filme);
    Task UpdateAsync(Filme filme);
    Task DeleteAsync(int id);
}
