using System.Text.Json.Serialization;

namespace CatalogoDeFilmes.Models
{
    // Resposta raiz da Open-Meteo
    public class WeatherForecastResponse
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("timezone_abbreviation")]
        public string? TimezoneAbbreviation { get; set; }

        [JsonPropertyName("elevation")]
        public double Elevation { get; set; }

        [JsonPropertyName("daily")]
        public WeatherDaily? Daily { get; set; }

        [JsonPropertyName("daily_units")]
        public WeatherDailyUnits? DailyUnits { get; set; }
    }

    // Bloco "daily" com listas de datas e temperaturas
    public class WeatherDaily
    {
        [JsonPropertyName("time")]
        public List<DateTime> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; } = new();
    }

    // Unidades (não é obrigatório usar, mas mapeei certinho)
    public class WeatherDailyUnits
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public string? TemperatureMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public string? TemperatureMin { get; set; }
    }
}
