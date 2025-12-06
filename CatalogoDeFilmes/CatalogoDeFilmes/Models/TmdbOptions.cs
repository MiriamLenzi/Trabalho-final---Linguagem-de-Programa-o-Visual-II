namespace CatalogoDeFilmes.Models;

public class TmdbOptions
{
    public bool UseBearerToken { get; set; }
    public string ApiKeyV3 { get; set; } = string.Empty;
    public string BearerTokenV4 { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ImageBaseUrlFallback { get; set; } = string.Empty;
    public string DefaultPosterSize { get; set; } = "w500";
}
