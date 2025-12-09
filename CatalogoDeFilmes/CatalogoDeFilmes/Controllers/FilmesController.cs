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
    private readonly ILogger<FilmesController> _logger;

    public FilmesController(
        IFilmeRepository repo,
        ITmdbApiService tmdb,
        IWeatherApiService weather,
        ILogger<FilmesController> logger)
    {
        _repo = repo;
        _tmdb = tmdb;
        _weather = weather;
        _logger = logger;
    }

    // LISTA LOCAL (CRUD)
    public async Task<IActionResult> Index()
    {
        try
        {
            var filmes = await _repo.ListAsync();
            return View(filmes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar filmes");
            TempData["Error"] = "Erro ao carregar a lista de filmes.";
            return View(new List<Filme>());
        }
    }

    // DETALHES (local + TMDb + tempo)
    public async Task<IActionResult> Details(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar detalhes do filme {FilmeId}", id);
            TempData["Error"] = "Erro ao carregar os detalhes do filme.";
            return RedirectToAction(nameof(Index));
        }
    }

    private string? BuildPosterUrl(TmdbConfiguration? config, string posterPath)
    {
        if (string.IsNullOrEmpty(posterPath)) return null;

        var baseUrl = config?.Images?.SecureBaseUrl
                      ?? config?.Images?.BaseUrl
                      ?? "https://image.tmdb.org/t/p/";

        var size = config?.Images?.PosterSizes?.Contains("w500") == true
            ? "w500"
            : config?.Images?.PosterSizes?.LastOrDefault() ?? "original";

        return $"{baseUrl}{size}{posterPath}";
    }

    // CREATE
    public IActionResult Create() => View(new Filme());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Filme filme)
    {
        try
        {
            // Se TmdbId não foi preenchido, usar um valor padrão (0 ou gerar um negativo)
            if (filme.TmdbId == 0)
            {
                // Para filmes manuais, vamos usar IDs negativos
                var filmes = await _repo.ListAsync();
                var minId = filmes.Where(f => f.TmdbId < 0).Select(f => f.TmdbId).DefaultIfEmpty(0).Min();
                filme.TmdbId = minId - 1;
            }

            if (!ModelState.IsValid) 
            {
                return View(filme);
            }

            await _repo.CreateAsync(filme);
            TempData["Message"] = "Filme criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar filme");
            ModelState.AddModelError("", "Erro ao criar o filme. Por favor, tente novamente.");
            return View(filme);
        }
    }

    // EDIT
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var filme = await _repo.GetByIdAsync(id);
            if (filme == null) return NotFound();
            return View(filme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar filme para edição {FilmeId}", id);
            TempData["Error"] = "Erro ao carregar o filme para edição.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Filme filme)
    {
        if (id != filme.Id) return BadRequest();
        
        try
        {
            if (!ModelState.IsValid) 
            {
                return View(filme);
            }

            await _repo.UpdateAsync(filme);
            TempData["Message"] = "Filme atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar filme {FilmeId}", id);
            ModelState.AddModelError("", "Erro ao atualizar o filme. Por favor, tente novamente.");
            return View(filme);
        }
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var filme = await _repo.GetByIdAsync(id);
            if (filme == null) return NotFound();
            return View(filme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar filme para exclusão {FilmeId}", id);
            TempData["Error"] = "Erro ao carregar o filme para exclusão.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _repo.DeleteAsync(id);
            TempData["Message"] = "Filme excluído com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir filme {FilmeId}", id);
            TempData["Error"] = "Erro ao excluir o filme. Por favor, tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    // BUSCA TMDb (RF02 + RF13)
    public async Task<IActionResult> SearchTmdb(string query, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ViewBag.Query = string.Empty;
            return View(new TmdbSearchResponse());
        }

        try
        {
            var result = await _tmdb.SearchMoviesAsync(query, page) ?? new TmdbSearchResponse();
            ViewBag.Query = query;
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar filmes no TMDb: {Query}", query);
            ViewBag.Error = "Erro ao buscar filmes no TMDb. Por favor, tente novamente.";
            ViewBag.Query = query;
            return View(new TmdbSearchResponse());
        }
    }

    // IMPORTAÇÃO (RF03)
    public async Task<IActionResult> ImportFromTmdb(int tmdbId)
    {
        try
        {
            var existing = await _repo.GetByTmdbIdAsync(tmdbId);
            if (existing != null)
            {
                TempData["Message"] = "Filme já importado anteriormente.";
                return RedirectToAction(nameof(Edit), new { id = existing.Id });
            }

            var details = await _tmdb.GetMovieDetailsAsync(tmdbId);
            if (details == null) 
            {
                TempData["Error"] = "Não foi possível obter os detalhes do filme no TMDb.";
                return RedirectToAction(nameof(SearchTmdb));
            }

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
            };

            return View("ImportFromTmdb", filme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar filme do TMDb {TmdbId}", tmdbId);
            TempData["Error"] = "Erro ao importar o filme. Por favor, tente novamente.";
            return RedirectToAction(nameof(SearchTmdb));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdbConfirmed(Filme filme)
    {
        try
        {
            if (!ModelState.IsValid) 
            {
                return View("ImportFromTmdb", filme);
            }

            await _repo.CreateAsync(filme);
            TempData["Message"] = "Filme importado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar importação do filme");
            ModelState.AddModelError("", "Erro ao importar o filme. Por favor, tente novamente.");
            return View("ImportFromTmdb", filme);
        }
    }

    // EXPORTAR CSV (RF12)
    public async Task<FileResult> ExportCsv()
    {
        try
        {
            var filmes = await _repo.ListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Titulo,TituloOriginal,DataLancamento,Genero,NotaMedia,CidadeReferencia,Latitude,Longitude");

            foreach (var f in filmes)
            {
                sb.AppendLine($"{f.Id},\"{EscapeCsv(f.Titulo)}\",\"{EscapeCsv(f.TituloOriginal)}\",{f.DataLancamento:yyyy-MM-dd},\"{EscapeCsv(f.Genero)}\",{f.NotaMedia},\"{EscapeCsv(f.CidadeReferencia)}\",{f.Latitude},{f.Longitude}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"catalogo_filmes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar CSV");
            throw;
        }
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }

    // EXPORTAR EXCEL simples usando ClosedXML
    public async Task<FileResult> ExportExcel()
    {
        try
        {
            var filmes = await _repo.ListAsync();

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Filmes");
            
            // Headers
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Título";
            ws.Cell(1, 3).Value = "Título Original";
            ws.Cell(1, 4).Value = "Data Lançamento";
            ws.Cell(1, 5).Value = "Gênero";
            ws.Cell(1, 6).Value = "Nota Média";
            ws.Cell(1, 7).Value = "Cidade";
            ws.Cell(1, 8).Value = "Latitude";
            ws.Cell(1, 9).Value = "Longitude";

            // Formatar header
            var headerRange = ws.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

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

            // Auto-ajustar colunas
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"catalogo_filmes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar Excel");
            throw;
        }
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