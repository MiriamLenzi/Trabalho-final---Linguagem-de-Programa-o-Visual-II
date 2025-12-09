🎬 Catálogo de Filmes + Previsão do Tempo

Este projeto é uma aplicação ASP.NET Core MVC que integra:

✔ Consumo da API TMDb para exibir filmes

✔ Consumo da API Open-Meteo para exibir previsão do tempo

✔ Componentes reutilizáveis (FilmeCard e WeatherBlock)

✔ Arquitetura organizada com Controllers, Models, Services e Views

✔ Armazenamento seguro de tokens usando .NET User Secrets

O objetivo é demonstrar o uso de APIs externas, boas práticas estruturais e segurança no desenvolvimento.

📂 Estrutura do Projeto
CatalogoDeFilmes/
 ├── Controllers/
 ├── Models/
 │    ├── WeatherDtos/
 │    ├── TmdbDtos/
 ├── Repositories/
 ├── Services/
 ├── Views/
 │    ├── Shared/
 │         ├── _FilmeCard.cshtml
 │         ├── _WeatherBlock.cshtml
 ├── wwwroot/
 ├── appsettings.json
 ├── Program.cs
 └── CatalogoDeFilmes.csproj

🛠 Tecnologias Utilizadas

ASP.NET Core MVC (.NET 9)

C#

TMDb API

Open-Meteo API

Bootstrap

View Components / Partial Views

User Secrets (para proteger tokens)

IDEs: JetBrains Rider ou Visual Studio

🔐 Configuração dos Tokens das APIs

Os tokens não devem ser colocados em appsettings.json, pois esse arquivo vai para o GitHub.

A forma correta é usar .NET User Secrets.

1️⃣ Gerar a chave da API TMDb

Acesse:
🔗 https://www.themoviedb.org/settings/api

Crie ou copie sua API Key v3
Exemplo:

c4bd1234e67f998a...

2️⃣ Armazenando o token com User Secrets
📌 No Rider

Clique com botão direito no projeto → Tools → .NET User Secrets

📌 No Visual Studio

Clique com botão direito no projeto → Manage User Secrets

📌 Cole o seguinte:
{
  "Tmdb": {
    "ApiKeyV3": "SUA-CHAVE-AQUI"
  },
  "Weather": {
    "BaseUrl": "https://api.open-meteo.com/v1/forecast"
  }
}


Agora o projeto poderá acessar sua chave sem expor no GitHub.

▶️ Como Executar o Projeto
dotnet restore
dotnet run


Ou use o botão Run da sua IDE.

A aplicação iniciará em algo como:

https://localhost:5001

🌦 APIs Utilizadas
🎬 TMDb — Filmes

Base URL:

https://api.themoviedb.org/3/

🌤 Open-Meteo — Clima

Base URL:

https://api.open-meteo.com/v1/forecast

⭐ Funcionalidades

Listagem de filmes

Exibição de posters com fallback

Previsão do tempo baseada na localização

Componentes reutilizáveis

Services organizando chamadas externas

Segurança com User Secrets

👩‍💻 Desenvolvedoras

Miriam Lenzi

Raiane Alves
