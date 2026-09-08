using backend.Models;
using backend.Services.Config;
using backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

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
            double promedioGolesLocal,
            double promedioGolesVisitante,
            double probLocal,
            double probEmpate,
            double probVisitante,
            string? competicion = null,
            string? estadio = null,
            string? bajas = null,
            string? contexto = null,
            List<PartidoHistorico>? historicoEnfrentamientos = null)
        {
            var prompt = new StringBuilder();

            // 1. Datos de la API
            prompt.AppendLine($"ANALISIS: {equipoLocal} vs {equipoVisitante}");
            prompt.AppendLine($"- Probabilidad {equipoLocal}: {probLocal:P0}");
            prompt.AppendLine($"- Probabilidad Empate: {probEmpate:P0}");
            prompt.AppendLine($"- Probabilidad {equipoVisitante}: {probVisitante:P0}");
            prompt.AppendLine($"- Promedio goles {equipoLocal}: {promedioGolesLocal:F2}");
            prompt.AppendLine($"- Promedio goles {equipoVisitante}: {promedioGolesVisitante:F2}");

            // 2. Datos manuales (desde el frontend)
            if (!string.IsNullOrEmpty(competicion))
                prompt.AppendLine($"- Competicion: {competicion}");
            if (!string.IsNullOrEmpty(estadio))
                prompt.AppendLine($"- Estadio: {estadio}");
            if (!string.IsNullOrEmpty(bajas))
                prompt.AppendLine($"- Bajas importantes: {bajas}");
            if (!string.IsNullOrEmpty(contexto))
                prompt.AppendLine($"- Contexto: {contexto}");

            // 3. Historial de enfrentamientos (Head-to-Head)
            if (historicoEnfrentamientos != null && historicoEnfrentamientos.Any())
            {
                prompt.AppendLine();
                prompt.AppendLine("HISTORIAL DE ENFRENTAMIENTOS DIRECTOS (ultimos 5):");
                foreach (var p in historicoEnfrentamientos.Take(5))
                {
                    prompt.AppendLine($"  {p.Local} {p.GolesLocal} - {p.GolesVisitante} {p.Visitante}");
                }

                // Estadisticas del H2H
                var totalPartidos = historicoEnfrentamientos.Count;
                var victoriasLocal = historicoEnfrentamientos.Count(p => p.GolesLocal > p.GolesVisitante);
                var victoriasVisitante = historicoEnfrentamientos.Count(p => p.GolesVisitante > p.GolesLocal);
                var empates = totalPartidos - victoriasLocal - victoriasVisitante;

                prompt.AppendLine();
                prompt.AppendLine("ESTADISTICAS H2H:");
                prompt.AppendLine($"- {equipoLocal} gano: {victoriasLocal} de {totalPartidos} ({victoriasLocal * 100 / totalPartidos}%)");
                prompt.AppendLine($"- Empates: {empates} de {totalPartidos} ({empates * 100 / totalPartidos}%)");
                prompt.AppendLine($"- {equipoVisitante} gano: {victoriasVisitante} de {totalPartidos} ({victoriasVisitante * 100 / totalPartidos}%)");

                // Promedio de goles en H2H
                var promedioGolesH2H = historicoEnfrentamientos.Average(p => p.GolesLocal + p.GolesVisitante);
                prompt.AppendLine($"- Promedio de goles por partido en H2H: {promedioGolesH2H:F2}");
            }

            // 4. Instrucciones para DeepSeek
            prompt.AppendLine();
            prompt.AppendLine("INSTRUCCIONES PARA EL ANALISIS:");
            prompt.AppendLine("1. Utiliza el historial de enfrentamientos directos como factor principal.");
            prompt.AppendLine("2. Si un equipo domina claramente el H2H, mencionalo en la recomendacion.");
            prompt.AppendLine("3. Si los enfrentamientos suelen tener muchos goles (>2.5), mencionalo.");
            prompt.AppendLine("4. No uses frases genericas como 'sera un partido parejo' sin justificacion.");
            prompt.AppendLine("5. Basa tu analisis en los datos proporcionados.");
            prompt.AppendLine("6. Responde EXCLUSIVAMENTE en el siguiente formato JSON:");
            prompt.AppendLine(@"
            {
              ""explicacion"": ""texto de maximo 4 lineas"",
              ""factores_clave"": [""factor1"", ""factor2"", ""factor3""],
              ""alertas"": [""alerta1"", ""alerta2""],
              ""recomendacion"": ""Victoria de X|Empate|Victoria de Y""
            }");

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
