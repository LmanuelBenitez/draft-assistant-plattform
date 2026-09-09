using backend.Models;

namespace backend.Services.Interfaces;

public interface IFootballApiService
{
    /// <summary>
    /// Obtiene los últimos partidos de un equipo
    /// </summary>
    Task<List<PartidoHistorico>> GetUltimosPartidosAsync(string equipo, string leagueId, string season, int limite = 10);

    /// <summary>
    /// Obtiene estadísticas de un equipo (promedio de goles, etc.)
    /// </summary>
    Task<EstadisticasEquipo> GetEstadisticasEquipoAsync(string equipo, string leagueId, string season);

    Task<List<PartidoHistorico>> GetPartidosHead2HeadAsync(string local, string visitante, string leagueIdLocal, string leagueIdVisitante, string season, int limite = 5);
}