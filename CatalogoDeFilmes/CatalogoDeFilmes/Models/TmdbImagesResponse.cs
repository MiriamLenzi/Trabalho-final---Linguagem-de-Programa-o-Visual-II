namespace CatalogoDeFilmes.Models;

public class TmdbImagesResponse
{
    public int Id { get; set; }
    public List<TmdbImageInfo> Backdrops { get; set; } = new();
    public List<TmdbImageInfo> Posters { get; set; } = new();
}

public class TmdbImageInfo
{
    public string? FilePath { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double? AspectRatio { get; set; }
    public double? VoteAverage { get; set; }
    public int VoteCount { get; set; }
}
