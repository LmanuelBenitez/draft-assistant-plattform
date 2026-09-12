using backend.Models;
using backend.Services.Config;
using backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace backend.Services.Implementations;
public class DeepSeekService : IDeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly DeepSeekConfig _config;
    private readonly ILogger<DeepSeekService> _logger;

    public DeepSeekService(HttpClient httpClient,
        IOptions<DeepSeekConfig> config,
        ILogger<DeepSeekService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> ObtenerPrediccionFutbolAsync(
        string equipoLocal,
        string equipoVisitante,
        EstadisticasEquipo statsLocal,
        EstadisticasEquipo statsVisitante,
        decimal probLocal,
        decimal probEmpate,
        decimal probVisitante,
        string? competicion = null,
        string? estadio = null,
        string? bajas = null,
        string? contexto = null,
        List<PartidoHistorico>? historicoEnfrentamientos = null)
        {
            var prompt = new StringBuilder();

            // ============================================
            // 1. DATOS DE PROBABILIDADES
            // ============================================
            prompt.AppendLine($"ANALISIS: {equipoLocal} vs {equipoVisitante}");
            prompt.AppendLine();
            prompt.AppendLine("PROBABILIDADES (modelo Poisson):");
            prompt.AppendLine($"- {equipoLocal}: {probLocal:P0}");
            prompt.AppendLine($"- Empate: {probEmpate:P0}");
            prompt.AppendLine($"- {equipoVisitante}: {probVisitante:P0}");

            // ============================================
            // 2. ESTADISTICAS DEL EQUIPO LOCAL
            // ============================================
            prompt.AppendLine();
            prompt.AppendLine($"ESTADISTICAS DE {equipoLocal}:");
            prompt.AppendLine($"- Ultimos {statsLocal.PartidosJugados} partidos: {statsLocal.Victorias}Victorias, {statsLocal.Empates}Empates, {statsLocal.Derrotas}Derrotas");
            prompt.AppendLine($"- Promedio goles a favor: {statsLocal.PromedioGolesFavor:F2}");
            prompt.AppendLine($"- Promedio goles en contra: {statsLocal.PromedioGolesContra:F2}");
            prompt.AppendLine($"- Racha reciente: {statsLocal.Racha}");
            prompt.AppendLine($"- Puntos por partido: {statsLocal.PuntosPorPartido}");
            prompt.AppendLine($"- Diferencia de goles: {statsLocal.DiferenciaGoles}");

            // ============================================
            // 3. ESTADISTICAS DEL EQUIPO VISITANTE
            // ============================================
            prompt.AppendLine();
            prompt.AppendLine($"ESTADISTICAS DE {equipoVisitante}:");
            prompt.AppendLine($"- Ultimos {statsVisitante.PartidosJugados} partidos: {statsVisitante.Victorias}Victorias, {statsVisitante.Empates}Empates, {statsVisitante.Derrotas}Derrotas");
            prompt.AppendLine($"- Promedio goles a favor: {statsVisitante.PromedioGolesFavor:F2}");
            prompt.AppendLine($"- Promedio goles en contra: {statsVisitante.PromedioGolesContra:F2}");
            prompt.AppendLine($"- Racha reciente: {statsVisitante.Racha}");
            prompt.AppendLine($"- Puntos por partido: {statsVisitante.PuntosPorPartido}");
            prompt.AppendLine($"- Diferencia de goles: {statsVisitante.DiferenciaGoles}");

            // ============================================
            // 4. DATOS MANUALES (desde el frontend)
            // ============================================
            if (!string.IsNullOrEmpty(competicion) ||
                !string.IsNullOrEmpty(estadio) ||
                !string.IsNullOrEmpty(bajas) ||
                !string.IsNullOrEmpty(contexto))
            {
                prompt.AppendLine();
                prompt.AppendLine("CONTEXTO DEL PARTIDO:");
                if (!string.IsNullOrEmpty(competicion))
                    prompt.AppendLine($"- Competicion: {competicion}");
                if (!string.IsNullOrEmpty(estadio))
                    prompt.AppendLine($"- Estadio: {estadio}");
                if (!string.IsNullOrEmpty(bajas))
                    prompt.AppendLine($"- Bajas importantes: {bajas}");
                if (!string.IsNullOrEmpty(contexto))
                    prompt.AppendLine($"- Contexto: {contexto}");
            }

            // ============================================
            // 5. HISTORIAL DE ENFRENTAMIENTOS (H2H)
            // ============================================
            if (historicoEnfrentamientos != null && historicoEnfrentamientos.Any())
            {
                prompt.AppendLine();
                prompt.AppendLine("HISTORIAL DE ENFRENTAMIENTOS DIRECTOS (ultimos 5):");
                foreach (var p in historicoEnfrentamientos)
                {
                    prompt.AppendLine($"  {p.Local} {p.GolesLocal} - {p.GolesVisitante} {p.Visitante}");
                }

                // Estadisticas del H2H
                var totalPartidos = historicoEnfrentamientos.Count;
                var victoriasLocal = historicoEnfrentamientos.Count(p =>
                    (p.Local == equipoLocal && p.GolesLocal > p.GolesVisitante) ||
                    (p.Visitante == equipoLocal && p.GolesVisitante > p.GolesLocal));

                var victoriasVisitante = historicoEnfrentamientos.Count(p =>
                    (p.Local == equipoVisitante && p.GolesLocal > p.GolesVisitante) ||
                    (p.Visitante == equipoVisitante && p.GolesVisitante > p.GolesLocal));

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

            // ============================================
            // 6. INSTRUCCIONES PARA DEEPSEEK
            // ============================================
            prompt.AppendLine();
            prompt.AppendLine("INSTRUCCIONES PARA EL ANALISIS:");
            prompt.AppendLine("1. Usa las estadisticas de ambos equipos como base principal.");
            prompt.AppendLine("2. Considera la racha y forma reciente como factor de momentum.");
            prompt.AppendLine("3. Usa el historial H2H para detectar patrones de enfrentamiento.");
            prompt.AppendLine("4. Si un equipo tiene mejor diferencia de goles, mencionalo.");
            prompt.AppendLine("5. No uses frases genericas como 'sera un partido parejo' sin justificacion.");
            prompt.AppendLine();
            prompt.AppendLine("FACTORES INTANGIBLES (opcionales):");
            prompt.AppendLine("6. Puedes mencionar factores intangibles SOLO si se derivan directamente de los datos proporcionados:");
            prompt.AppendLine("   - Racha actual: basate en los ultimos 5 resultados que tienes");
            prompt.AppendLine("   - Moral: basate en la diferencia de goles y resultados recientes");
            prompt.AppendLine("   - Factor psicologico H2H: basate en el historial directo");
            prompt.AppendLine("   - Presion del partido: basate en el contexto proporcionado (localia, competicion)");
            prompt.AppendLine("   - Importancia: basate en el contexto proporcionado");
            prompt.AppendLine();
            prompt.AppendLine("7. IMPORTANTE: Si no tienes datos suficientes para justificar un factor intangible,");
            prompt.AppendLine("   NO lo menciones. Es mejor un analisis corto y solido que uno largo e inventado.");
            prompt.AppendLine();
            prompt.AppendLine("8. Si en tu analisis mencionas que el empate es probable, tu recomendacion DEBE ser Empate.");
            prompt.AppendLine("9. Responde EXCLUSIVAMENTE en el siguiente formato JSON:");
            prompt.AppendLine(@"
            {
                ""explicacion"": ""texto de maximo 5 lineas"",
                ""factores_clave"": [""factor1"", ""factor2"", ""factor3""],
                ""alertas"": [""alerta1"", ""alerta2""],
                ""recomendacion"": ""Victoria de X|Empate|Victoria de Y""
            }");

        return await ConsultarAsync(prompt.ToString(), 500);
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
                thinking = new { type = "disabled" },
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

