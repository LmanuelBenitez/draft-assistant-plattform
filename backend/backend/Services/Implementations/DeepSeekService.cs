using System.Text;
using System.Text.Json;
using backend.Services.Config;
using backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace backend.Services.Implementations
{
    public class DeepSeekService(
        HttpClient httpClient,
        IOptions<DeepSeekConfig> config,
        ILogger<DeepSeekService> logger) : IDeepSeekService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly DeepSeekConfig _config = config.Value;
        private readonly ILogger<DeepSeekService> _logger = logger;

        public DeepSeekService() : this(null!, null!, null!)
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        }

        public async Task<string> ConsultarAsync(string prompt, int maxTokens = 500)
        {
            try
            {
                var request = new
                {
                    model = _config.Model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = maxTokens,
                    temperature = _config.Temperature,
                    top_p = _config.TopP
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var result = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return result ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar DeepSeek API");
                throw;
            }
        }

        public async Task<string> ObtenerPrediccionFutbolAsync(
            string equipoLocal,
            string equipoVisitante,
            string? estadio = null,
            string? contexto = null)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine($"Realiza un análisis detallado para el partido de fútbol entre {equipoLocal} y {equipoVisitante}.");
            prompt.AppendLine();

            if (!string.IsNullOrEmpty(estadio))
                prompt.AppendLine($"Estadio: {estadio}");

            if (!string.IsNullOrEmpty(contexto))
                prompt.AppendLine($"Contexto adicional: {contexto}");

            prompt.AppendLine();
            prompt.AppendLine("Proporciona la predicción en formato JSON con la siguiente estructura:");
            prompt.AppendLine("{");
            prompt.AppendLine("  \"goles_local\": numero,");
            prompt.AppendLine("  \"goles_visitante\": numero,");
            prompt.AppendLine("  \"probabilidad_local\": decimal,");
            prompt.AppendLine("  \"probabilidad_empate\": decimal,");
            prompt.AppendLine("  \"probabilidad_visitante\": decimal,");
            prompt.AppendLine("  \"analisis\": \"texto explicativo\",");
            prompt.AppendLine("  \"factores_clave\": [\"factor1\", \"factor2\"],");
            prompt.AppendLine("  \"recomendacion\": \"recomendación\"");
            prompt.AppendLine("}");

            return await ConsultarAsync(prompt.ToString(), 800);
        }

        public async Task<string> AnalizarEquipoAsync(string equipoNombre)
        {
            var prompt = $@"Realiza un análisis del equipo de fútbol {equipoNombre}. Proporciona la respuesta en formato JSON con:
            {{
                ""fortalezas"": [""fortaleza1"", ""fortaleza2""],
                ""debilidades"": [""debilidad1"", ""debilidad2""],
                ""estilo_juego"": ""descripción del estilo"",
                ""promedio_goles_local"": decimal,
                ""promedio_goles_visitante"": decimal,
                ""rendimiento_reciente"": ""bueno|regular|malo""
            }}";

            return await ConsultarAsync(prompt, 600);
        }

        public async Task<bool> VerificarDisponibilidadAsync()
        {
            try
            {
                var request = new
                {
                    model = _config.Model,
                    messages = new[]
                    {
                        new { role = "user", content = "Test" }
                    },
                    max_tokens = 1
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/chat/completions", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
