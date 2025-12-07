using System.Text;
using CatalogoDeFilmes.Models;
using CatalogoDeFilmes.Repositories;
using CatalogoDeFilmes.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoDeFilmes.Controllers;

public class FilmesController : Controller
{
    private readonly IFilmeRepository _repo;
    private readonly ITmdbApiService _tmdb;
    private readonly IWeatherApiService _weather;

    public FilmesController(
        IFilmeRepository repo,
        ITmdbApiService tmdb,
        IWeatherApiService weather)
    {
        _repo = repo;
        _tmdb = tmdb;
        _weather = weather;
    }

    // LISTA LOCAL (CRUD)
    public async Task<IActionResult> Index()
    {
        var filmes = await _repo.ListAsync();
        return View(filmes);
    }

    // DETALHES (local + TMDb + tempo)
    public async Task<IActionResult> Details(int id)
    {
        var filme = await _repo.GetByIdAsync(id);
        if (filme == null) return NotFound();

        TmdbMovieDetails? details = await _tmdb.GetMovieDetailsAsync(filme.TmdbId);
        WeatherForecastResponse? weather = null;

        if (filme.Latitude.HasValue && filme.Longitude.HasValue)
        {
            weather = await _weather.GetDailyForecastAsync(filme.Latitude.Value, filme.Longitude.Value);
        }

        var config = await _tmdb.GetConfigurationAsync();
        var posterUrl = BuildPosterUrl(config, filme.PosterPath);

        var vm = new FilmeDetalhesViewModel
        {
            Filme = filme,
            TmdbDetails = details,
            PosterUrl = posterUrl,
            Weather = weather
        };

        return View(vm);
    }

    private string? BuildPosterUrl(TmdbConfiguration? config, string posterPath)
    {
        if (string.IsNullOrEmpty(posterPath)) return null;

        var baseUrl = config?.Images?.SecureBaseUrl
                      ?? config?.Images?.BaseUrl
                      ?? "/"; // fallback

        var size = config?.Images?.PosterSizes.Contains("w500") == true
            ? "w500"
            : config?.Images?.PosterSizes.LastOrDefault() ?? "original";

        return $"{baseUrl}{size}{posterPath}";
    }

    // CREATE
    public IActionResult Create() => View(new Filme());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Filme filme)
    {
        if (!ModelState.IsValid) return View(filme);

        await _repo.CreateAsync(filme);
        return RedirectToAction(nameof(Index));
    }

    // EDIT
    public async Task<IActionResult> Edit(int id)
    {
        var filme = await _repo.GetByIdAsync(id);
        if (filme == null) return NotFound();
        return View(filme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Filme filme)
    {
        if (id != filme.Id) return BadRequest();
        if (!ModelState.IsValid) return View(filme);

        await _repo.UpdateAsync(filme);
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var filme = await _repo.GetByIdAsync(id);
        if (filme == null) return NotFound();
        return View(filme);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // BUSCA TMDb (RF02 + RF13)
    public async Task<IActionResult> SearchTmdb(string query, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ViewBag.Query = string.Empty;
            return View(new TmdbSearchResponse());
        }

        var result = await _tmdb.SearchMoviesAsync(query, page) ?? new TmdbSearchResponse();
        ViewBag.Query = query;
        return View(result);
    }

    // IMPORTAÇÃO (RF03)
    public async Task<IActionResult> ImportFromTmdb(int tmdbId)
    {
        var existing = await _repo.GetByTmdbIdAsync(tmdbId);
        if (existing != null)
        {
            TempData["Message"] = "Filme já importado.";
            return RedirectToAction(nameof(Edit), new { id = existing.Id });
        }

        var details = await _tmdb.GetMovieDetailsAsync(tmdbId);
        if (details == null) return NotFound();

        var genero = string.Join(", ", details.Genres.Select(g => g.Name));
        var elenco = string.Join(", ", details.Credits?.Cast
            .Take(5)
            .Select(c => c.Name) ?? Array.Empty<string>());

        var filme = new Filme
        {
            TmdbId = details.Id,
            Titulo = details.Title,
            TituloOriginal = details.OriginalTitle,
            Sinopse = details.Overview,
            DataLancamento = string.IsNullOrEmpty(details.ReleaseDate)
                ? null
                : DateTime.Parse(details.ReleaseDate),
            Genero = genero,
            PosterPath = details.PosterPath ?? string.Empty,
            Lingua = details.OriginalLanguage,
            Duracao = details.Runtime,
            NotaMedia = details.VoteAverage,
            ElencoPrincipal = elenco,
            // CidadeReferencia / Lat / Long: usuário completa manualmente
        };

        // Mostra view para o usuário completar cidade/lat/long e salvar
        return View("ImportFromTmdb", filme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdbConfirmed(Filme filme)
    {
        if (!ModelState.IsValid) return View("ImportFromTmdb", filme);

        await _repo.CreateAsync(filme);
        TempData["Message"] = "Filme importado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // EXPORTAR CSV (RF12)
    public async Task<FileResult> ExportCsv()
    {
        var filmes = await _repo.ListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,Titulo,TituloOriginal,DataLancamento,Genero,NotaMedia,CidadeReferencia,Latitude,Longitude");

        foreach (var f in filmes)
        {
            sb.AppendLine($"{f.Id},\"{f.Titulo}\",\"{f.TituloOriginal}\",{f.DataLancamento:yyyy-MM-dd},\"{f.Genero}\",{f.NotaMedia},{f.CidadeReferencia},{f.Latitude},{f.Longitude}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "catalogo_filmes.csv");
    }

    // EXPORTAR EXCEL simples usando ClosedXML (instalar pacote ClosedXML)
    public async Task<FileResult> ExportExcel()
    {
        var filmes = await _repo.ListAsync();

        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.Worksheets.Add("Filmes");
        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "Título";
        ws.Cell(1, 3).Value = "Título Original";
        ws.Cell(1, 4).Value = "Data Lançamento";
        ws.Cell(1, 5).Value = "Gênero";
        ws.Cell(1, 6).Value = "Nota Média";
        ws.Cell(1, 7).Value = "Cidade";
        ws.Cell(1, 8).Value = "Latitude";
        ws.Cell(1, 9).Value = "Longitude";

        var row = 2;
        foreach (var f in filmes)
        {
            ws.Cell(row, 1).Value = f.Id;
            ws.Cell(row, 2).Value = f.Titulo;
            ws.Cell(row, 3).Value = f.TituloOriginal;
            ws.Cell(row, 4).Value = f.DataLancamento;
            ws.Cell(row, 5).Value = f.Genero;
            ws.Cell(row, 6).Value = f.NotaMedia;
            ws.Cell(row, 7).Value = f.CidadeReferencia;
            ws.Cell(row, 8).Value = f.Latitude;
            ws.Cell(row, 9).Value = f.Longitude;
            row++;
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "catalogo_filmes.xlsx");
    }
}

// ViewModel usada em Details
public class FilmeDetalhesViewModel
{
    public Filme Filme { get; set; } = null!;
    public TmdbMovieDetails? TmdbDetails { get; set; }
    public string? PosterUrl { get; set; }
    public WeatherForecastResponse? Weather { get; set; }
}
