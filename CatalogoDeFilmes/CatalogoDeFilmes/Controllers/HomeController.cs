using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CatalogoDeFilmes.Models;

namespace CatalogoDeFilmes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Quando acessar / ou /Home/Index, vai direto para o catálogo de filmes
        public IActionResult Index()
        {
            // Opcional: logar o acesso
            _logger.LogInformation("Redirecionando da Home para Filmes/Index");
            return RedirectToAction("Index", "Filmes");
        }

        // Se ainda quiser manter a página de privacidade, pode deixar assim:
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
