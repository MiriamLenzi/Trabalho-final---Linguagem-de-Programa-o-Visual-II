namespace CatalogoDeFilmes.Models;

public class Filme
{
    public int Id { get; set; }
    public int TmdbId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string TituloOriginal { get; set; } = string.Empty;
    public string Sinopse { get; set; } = string.Empty;

    public DateTime? DataLancamento { get; set; }
    public string Genero { get; set; } = string.Empty;

    public string PosterPath { get; set; } = string.Empty;
    public string Lingua { get; set; } = string.Empty;
    public int? Duracao { get; set; }
    public double? NotaMedia { get; set; }

    public string ElencoPrincipal { get; set; } = string.Empty;

    public string CidadeReferencia { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
}