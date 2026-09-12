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
    }

    public async Task<EstadisticasEquipo> GetEstadisticasEquipoAsync(string equipo, string leagueId, string season)
    {
        try
        {
            var partidos = await GetUltimosPartidosAsync(equipo, leagueId, season, 10);

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

            var rachaReciente = await GetRachaRecienteAsync(equipo, partidos);

            var comparer = StringComparison.OrdinalIgnoreCase;

            var golesFavor = partidos
                .Select(p => p.Local.Equals(equipo, comparer) ? p.GolesLocal : p.GolesVisitante)
                .ToList();

            var golesContra = partidos
                .Select(p => p.Local.Equals(equipo, comparer) ? p.GolesVisitante : p.GolesLocal)
                .ToList();

            var equipoId = partidos.First().Local.Equals(equipo, comparer) ? partidos.First().LocalId : partidos.First().VisitanteId;

            // Calcular victorias, empates, derrotas
            var victorias = golesFavor.Where((g, i) => g > golesContra[i]).Count();
            var empates = golesFavor.Where((g, i) => g == golesContra[i]).Count();
            var derrotas = golesFavor.Where((g, i) => g < golesContra[i]).Count();
            
            var puntosPorPartido = ((victorias * 3.0) + empates) / partidos.Count;
            var diferenciaGoles = golesFavor.Sum() - golesContra.Sum();

            return new EstadisticasEquipo
            {
                EquipoId = equipoId.ToString(),
                Equipo = equipo,
                PartidosJugados = partidos.Count,
                PromedioGolesFavor = golesFavor.Average(),
                PromedioGolesContra = golesContra.Average(),
                Victorias = victorias,
                Empates = empates,
                Derrotas = derrotas,
                GolesPorPartido = golesFavor,
                Racha = rachaReciente,
                PuntosPorPartido = puntosPorPartido,
                DiferenciaGoles = diferenciaGoles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de {Equipo}", equipo);
            throw;
        }
    }

    public async Task<List<PartidoHistorico>> GetUltimosPartidosAsync(string equipo, string leagueId, string season, int limite = 10)
    {
        try
        {
            // Buscar equipo por nombre
            var teamId = await GetTeamIdAsync(equipo, leagueId, season);
            if (teamId == 0)
            {
                _logger.LogWarning("Equipo no encontrado: {Equipo}", equipo);
                return new List<PartidoHistorico>();
            }

            // Obtener partidos del equipo
            var url = $"fixtures?team={teamId}&status=FT-AET-PEN&last={limite}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FootballApiResponse>(json);

            if (result?.Response == null || !result.Response.Any())
                return new List<PartidoHistorico>();

            return result.Response.Select(p => new PartidoHistorico
            {
                LocalId = p.Teams.Home.Id,
                VisitanteId = p.Teams.Away.Id,
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

    public async Task<List<PartidoHistorico>> GetPartidosHead2HeadAsync(string localId, string visitanteId, string leagueId, string season, int limite = 5)
    {
        try
        {
            
            // Ejemplo: fixtures/headtohead?h2h=541-529&limit=5
            var url = $"fixtures/headtohead?h2h={localId}-{visitanteId}&status=FT-AET-PEN&last={limite}";

            // Hacer la petición a la API
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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error HTTP al obtener Head-to-Head entre {Local} y {Visitante}", localId, visitanteId);
            return new List<PartidoHistorico>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear JSON de Head-to-Head entre {Local} y {Visitante}", localId, visitanteId);
            return new List<PartidoHistorico>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener Head-to-Head entre {Local} y {Visitante}", localId, visitanteId);
            return new List<PartidoHistorico>();
        }
    }

    private async Task<int> GetTeamIdAsync(string nombreEquipo, string leagueId, string season)
    {
        try
        {
            var url = $"teams?name={Uri.EscapeDataString(nombreEquipo)}&league={leagueId}&season={season}";
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

    private async Task<string> GetRachaRecienteAsync(string equipo, List<PartidoHistorico> partidos)
    {
        var racha = partidos
            .OrderByDescending(p => p.Fecha)
            .Take(5)
            .Select(p => 
            {
                var esLocal = p.Local == equipo;
                var gf = esLocal ? p.GolesLocal : p.GolesVisitante;
                var gc = esLocal ? p.GolesVisitante : p.GolesLocal;

                if (gf > gc) return "V";
                if (gf == gc) return "E";
                return "D";
            });

        return string.Join("-", racha);  // Ej: "V-V-E-D-V"
    }
}
