using System.Text.Json;
using backend.Models;
using backend.Services.Config;
using backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace backend.Services.Implementations;

public class FootballApiService : IFootballApiService
{
    private readonly HttpClient _httpClient;
    private readonly FootballApiConfig _config;
    private readonly ILogger<FootballApiService> _logger;

    public FootballApiService(
        HttpClient httpClient,
        IOptions<FootballApiConfig> config,
        ILogger<FootballApiService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        // Configurar HttpClient
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-apisports-key", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<List<PartidoHistorico>> GetUltimosPartidosAsync(string equipo, int limite = 10)
    {
        try
        {
            // Buscar equipo por nombre
            var teamId = await GetTeamIdAsync(equipo);
            if (teamId == 0)
            {
                _logger.LogWarning("Equipo no encontrado: {Equipo}", equipo);
                return new List<PartidoHistorico>();
            }

            // Obtener partidos del equipo
            var url = $"fixtures?team={teamId}&league={_config.LeagueId}&season={_config.Season}&status=FT&limit={limite}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FootballApiResponse>(json);

            if (result?.Response == null || !result.Response.Any())
                return new List<PartidoHistorico>();

            return result.Response.Select(p => new PartidoHistorico
            {
                Local = p.Teams.Home.Name,
                Visitante = p.Teams.Away.Name,
                GolesLocal = p.Goals.Home,
                GolesVisitante = p.Goals.Away,
                Fecha = p.Fixture.Date
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidos de {Equipo}", equipo);
            throw;
        }
    }

    public async Task<EstadisticasEquipo> GetEstadisticasEquipoAsync(string equipo)
    {
        try
        {
            var partidos = await GetUltimosPartidosAsync(equipo, 10);

            if (!partidos.Any())
            {
                return new EstadisticasEquipo
                {
                    Equipo = equipo,
                    PartidosJugados = 0,
                    PromedioGolesFavor = 1.0,
                    PromedioGolesContra = 1.0
                };
            }

            var golesFavor = partidos.Select(p => p.GolesLocal).ToList();
            var golesContra = partidos.Select(p => p.GolesVisitante).ToList();

            // Calcular victorias, empates, derrotas
            var victorias = partidos.Count(p => p.GolesLocal > p.GolesVisitante);
            var empates = partidos.Count(p => p.GolesLocal == p.GolesVisitante);
            var derrotas = partidos.Count(p => p.GolesLocal < p.GolesVisitante);

            return new EstadisticasEquipo
            {
                Equipo = equipo,
                PartidosJugados = partidos.Count,
                PromedioGolesFavor = golesFavor.Average(),
                PromedioGolesContra = golesContra.Average(),
                Victorias = victorias,
                Empates = empates,
                Derrotas = derrotas,
                GolesPorPartido = golesFavor
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de {Equipo}", equipo);
            throw;
        }
    }

    private async Task<int> GetTeamIdAsync(string nombreEquipo)
    {
        try
        {
            var url = $"teams?name={Uri.EscapeDataString(nombreEquipo)}&league={_config.LeagueId}&season={_config.Season}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("response", out var teams) && teams.GetArrayLength() > 0)
            {
                var team = teams[0];
                if (team.TryGetProperty("team", out var teamInfo) &&
                    teamInfo.TryGetProperty("id", out var id))
                {
                    return id.GetInt32();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ID del equipo {Equipo}", nombreEquipo);
            return 0;
        }
    }
}
