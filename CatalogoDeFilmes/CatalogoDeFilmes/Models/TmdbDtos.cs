using System.Text.Json.Serialization;

namespace CatalogoDeFilmes.Models;

// SEARCH
public class TmdbSearchResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("results")]
    public List<TmdbMovieSummary> Results { get; set; } = new();
}

public class TmdbMovieSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
}

// DETAILS
public class TmdbMovieDetails : TmdbMovieSummary
{
    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = new();

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }

    // Opcional: se TMDb não trouxer lat/long, usuário preenche na importação
}

public class TmdbGenre
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbCredits
{
    [JsonPropertyName("cast")]
    public List<TmdbCastMember> Cast { get; set; } = new();
}

public class TmdbCastMember
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// CONFIGURAÇÃO (para montar poster URL)
public class TmdbConfiguration
{
    [JsonPropertyName("images")]
    public TmdbImageConfig Images { get; set; } = new();
}

public class TmdbImageConfig
{
    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("secure_base_url")]
    public string SecureBaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("poster_sizes")]
    public List<string> PosterSizes { get; set; } = new();
}
