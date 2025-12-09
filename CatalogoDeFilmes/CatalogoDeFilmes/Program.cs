using CatalogoDeFilmes.Models;
using CatalogoDeFilmes.Repositories;
using CatalogoDeFilmes.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Cache
builder.Services.AddMemoryCache();

// Config TMDb (usa user-secrets, não commitar)
builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("Tmdb"));

// HttpClients
builder.Services.AddHttpClient<ITmdbApiService, TmdbApiService>();
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>();

// Repositório (SQLite simples, sem migrations)
builder.Services.AddSingleton<IFilmeRepository, FilmeRepository>();

var app = builder.Build();

// Pipeline padrão
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Filmes}/{action=Index}/{id?}");

app.Run();
