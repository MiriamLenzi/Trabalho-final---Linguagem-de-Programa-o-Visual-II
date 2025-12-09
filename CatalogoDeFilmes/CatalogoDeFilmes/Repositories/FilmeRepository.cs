using CatalogoDeFilmes.Models;
using Microsoft.Data.Sqlite;

namespace CatalogoDeFilmes.Repositories;

public class FilmeRepository : IFilmeRepository
{
    private readonly string _connectionString;

    public FilmeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FilmesDb")
                            ?? "Data Source=filmes.db";
        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Filmes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TmdbId INTEGER NOT NULL,
                Titulo TEXT NOT NULL,
                TituloOriginal TEXT,
                Sinopse TEXT,
                DataLancamento TEXT,
                Genero TEXT,
                PosterPath TEXT,
                Lingua TEXT,
                Duracao INTEGER,
                NotaMedia REAL,
                ElencoPrincipal TEXT,
                CidadeReferencia TEXT,
                Latitude REAL,
                Longitude REAL,
                DataCriacao TEXT,
                DataAtualizacao TEXT
            );";
        cmd.ExecuteNonQuery();
    }

    private Filme Map(SqliteDataReader reader)
    {
        return new Filme
        {
            Id = reader.GetInt32(0),
            TmdbId = reader.GetInt32(1),
            Titulo = reader.GetString(2),
            TituloOriginal = reader.GetString(3),
            Sinopse = reader.GetString(4),
            DataLancamento = string.IsNullOrEmpty(reader.GetString(5)) ? null :
                DateTime.Parse(reader.GetString(5)),
            Genero = reader.GetString(6),
            PosterPath = reader.GetString(7),
            Lingua = reader.GetString(8),
            Duracao = reader.IsDBNull(9) ? null : reader.GetInt32(9),
            NotaMedia = reader.IsDBNull(10) ? null : reader.GetDouble(10),
            ElencoPrincipal = reader.GetString(11),
            CidadeReferencia = reader.GetString(12),
            Latitude = reader.IsDBNull(13) ? null : reader.GetDouble(13),
            Longitude = reader.IsDBNull(14) ? null : reader.GetDouble(14),
            DataCriacao = DateTime.Parse(reader.GetString(15)),
            DataAtualizacao = string.IsNullOrEmpty(reader.GetString(16)) ? null :
                DateTime.Parse(reader.GetString(16))
        };
    }

    public async Task<List<Filme>> ListAsync()
    {
        var filmes = new List<Filme>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Filmes ORDER BY DataCriacao DESC";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            filmes.Add(Map(reader));
        }

        return filmes;
    }

    public async Task<Filme?> GetByIdAsync(int id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Filmes WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<Filme?> GetByTmdbIdAsync(int tmdbId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Filmes WHERE TmdbId = $tmdbId";
        cmd.Parameters.AddWithValue("$tmdbId", tmdbId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task CreateAsync(Filme filme)
    {
        filme.DataCriacao = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Filmes 
            (TmdbId, Titulo, TituloOriginal, Sinopse, DataLancamento, Genero,
             PosterPath, Lingua, Duracao, NotaMedia, ElencoPrincipal,
             CidadeReferencia, Latitude, Longitude, DataCriacao, DataAtualizacao)
            VALUES
            ($TmdbId, $Titulo, $TituloOriginal, $Sinopse, $DataLancamento, $Genero,
             $PosterPath, $Lingua, $Duracao, $NotaMedia, $ElencoPrincipal,
             $CidadeReferencia, $Latitude, $Longitude, $DataCriacao, $DataAtualizacao);";

        AddParameters(cmd, filme);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Filme filme)
    {
        filme.DataAtualizacao = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Filmes SET
                TmdbId = $TmdbId,
                Titulo = $Titulo,
                TituloOriginal = $TituloOriginal,
                Sinopse = $Sinopse,
                DataLancamento = $DataLancamento,
                Genero = $Genero,
                PosterPath = $PosterPath,
                Lingua = $Lingua,
                Duracao = $Duracao,
                NotaMedia = $NotaMedia,
                ElencoPrincipal = $ElencoPrincipal,
                CidadeReferencia = $CidadeReferencia,
                Latitude = $Latitude,
                Longitude = $Longitude,
                DataCriacao = $DataCriacao,
                DataAtualizacao = $DataAtualizacao
            WHERE Id = $Id";

        AddParameters(cmd, filme);
        cmd.Parameters.AddWithValue("$Id", filme.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Filmes WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParameters(SqliteCommand cmd, Filme f)
    {
        cmd.Parameters.AddWithValue("$TmdbId", f.TmdbId);
        cmd.Parameters.AddWithValue("$Titulo", f.Titulo);
        cmd.Parameters.AddWithValue("$TituloOriginal", f.TituloOriginal);
        cmd.Parameters.AddWithValue("$Sinopse", f.Sinopse);
        cmd.Parameters.AddWithValue("$DataLancamento", f.DataLancamento?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("$Genero", f.Genero);
        cmd.Parameters.AddWithValue("$PosterPath", f.PosterPath);
        cmd.Parameters.AddWithValue("$Lingua", f.Lingua);
        cmd.Parameters.AddWithValue("$Duracao", (object?)f.Duracao ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$NotaMedia", (object?)f.NotaMedia ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$ElencoPrincipal", f.ElencoPrincipal);
        cmd.Parameters.AddWithValue("$CidadeReferencia", f.CidadeReferencia);
        cmd.Parameters.AddWithValue("$Latitude", (object?)f.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Longitude", (object?)f.Longitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$DataCriacao", f.DataCriacao.ToString("O"));
        cmd.Parameters.AddWithValue("$DataAtualizacao", f.DataAtualizacao?.ToString("O") ?? "");
    }
}
