using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Models;
using backend.Data;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.Implementations
{
    public class PrediccionService(
        AppDbContext context,
        IPoissonService poissonService,
        IDeepSeekService deepSeekService,
        IFootballApiService footballApiService,
        ILogger<PrediccionService> logger) : IPrediccionService
    {
        private readonly AppDbContext _context = context;
        private readonly IPoissonService _poissonService = poissonService;
        private readonly IDeepSeekService _deepSeekService = deepSeekService;
        private readonly IFootballApiService _footballApi = footballApiService;
        private readonly ILogger<PrediccionService> _logger = logger;

        /// <summary>
        /// Genera una predicción a partir de los nombres de los equipos
        /// </summary>
        public async Task<PrediccionResponseDto> GenerarPrediccionAsync(PartidoRequestDto request)
        {
            try
            {
                // 1. Validar entrada
                if (string.IsNullOrWhiteSpace(request.Local) || string.IsNullOrWhiteSpace(request.Visitante))
                    throw new ArgumentException("Local y Visitante son requeridos");

                if (request.Local.Length < 2 || request.Visitante.Length < 2)
                    throw new ArgumentException("Los equipos deben tener al menos 2 caracteres");

                // 2. Obtener estadísticas reales desde API-Football
                var statsLocal = await _footballApi.GetEstadisticasEquipoAsync(request.Local);
                var statsVisitante = await _footballApi.GetEstadisticasEquipoAsync(request.Visitante);

                var promedioLocal = statsLocal.PromedioGolesFavor > 0 ? statsLocal.PromedioGolesFavor : 1.0;
                var promedioVisitante = statsVisitante.PromedioGolesFavor > 0 ? statsVisitante.PromedioGolesFavor : 1.0;

                _logger.LogInformation("Promedios: {Local} = {PromLocal}, {Visitante} = {PromVisit}",
                    request.Local, promedioLocal, request.Visitante, promedioVisitante);

                // 3. Calcular predicción con Poisson
                var marcador = await _poissonService.PredecirMarcadorAsync(promedioLocal, promedioVisitante);
                var probabilidades = await _poissonService.CalcularProbabilidadesPartidoAsync(promedioLocal, promedioVisitante);
                var confianza = await _poissonService.CalcularConfianzaAsync(
                    promedioLocal,
                    promedioVisitante,
                    marcador.GolesLocal,
                    marcador.GolesVisitante);

                // 4. Buscar o crear partido en la base de datos
                var partido = await _context.Partidos
                    .FirstOrDefaultAsync(p =>
                        p.Local == request.Local &&
                        p.Visitante == request.Visitante &&
                        p.FechaHora.Date == DateTime.UtcNow.Date)  // Para evitar duplicados el mismo día
                    ?? new Partido
                    {
                        Local = request.Local,
                        Visitante = request.Visitante,
                        FechaHora = DateTime.UtcNow,
                        Estado = "Pendiente"
                    };

                if (partido.Id == 0)
                {
                    _context.Partidos.Add(partido);
                    await _context.SaveChangesAsync();
                }

                // 5. Crear y guardar la predicción
                var prediccion = new Prediccion
                {
                    PartidoId = partido.Id,
                    GolesLocalPredichos = marcador.GolesLocal,
                    GolesVisitantePredichos = marcador.GolesVisitante,
                    ProbabilidadLocal = (decimal)probabilidades.Local,
                    ProbabilidadEmpate = (decimal)probabilidades.Empate,
                    ProbabilidadVisitante = (decimal)probabilidades.Visitante,
                    Confianza = (decimal)confianza,
                    FechaPrediccion = DateTime.UtcNow
                };

                _context.Predicciones.Add(prediccion);
                await _context.SaveChangesAsync();

                // 6. Obtener análisis de DeepSeek (en segundo plano)
                try
                {
                    var deepSeekResponse = await _deepSeekService.ObtenerPrediccionFutbolAsync(
                        request.Local,
                        request.Visitante);

                    prediccion.Comentarios = deepSeekResponse;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al obtener predicción de DeepSeek");
                }

                // 7. Devolver respuesta
                return await MapToResponseDto(prediccion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar predicción para {request.Local} vs {request.Visitante}");
                throw;
            }
        }

        public async Task<PrediccionResponseDto> ObtenerPrediccionAsync(int id)
        {
            var prediccion = await _context.Predicciones
                .Include(p => p.Partido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prediccion == null)
                throw new ArgumentException($"Predicción con ID {id} no encontrada");

            return await MapToResponseDto(prediccion);
        }

        public async IAsyncEnumerable<PrediccionResponseDto> ObtenerPrediccionesPorPartidoAsync(int partidoId)
        {
            await foreach (var p in _context.Predicciones
                .Include(p => p.Partido)
                .Where(p => p.PartidoId == partidoId)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable())
            {
                yield return await MapToResponseDto(p);
            }
        }

        public async Task<bool> ValidarPrediccionAsync(PrediccionResponseDto prediccion)
        {
            if (prediccion.GolesLocalPredichos < 0 || prediccion.GolesVisitantePredichos < 0)
                return false;

            if (prediccion.GolesLocalPredichos > 10 || prediccion.GolesVisitantePredichos > 10)
                return false;

            if (prediccion.Confianza < 0 || prediccion.Confianza > 1)
                return false;

            var suma = (prediccion.ProbabilidadLocal ?? 0) +
                      (prediccion.ProbabilidadEmpate ?? 0) +
                      (prediccion.ProbabilidadVisitante ?? 0);

            return Math.Abs(suma - 1) < 0.01m;
        }

        public async Task<decimal> CalcularConfianzaAsync(PrediccionResponseDto prediccion)
        {
            var confianzaBase = prediccion.Confianza ?? 0.5m;

            if (prediccion.ProbabilidadLocal.HasValue &&
                prediccion.ProbabilidadVisitante.HasValue &&
                prediccion.ProbabilidadEmpate.HasValue)
            {
                var maxProb = Math.Max(
                    Math.Max(prediccion.ProbabilidadLocal.Value, prediccion.ProbabilidadVisitante.Value),
                    prediccion.ProbabilidadEmpate.Value);

                var ajuste = (maxProb - 0.33m) * 1.5m;
                confianzaBase = Math.Min(confianzaBase + ajuste, 0.95m);
                confianzaBase = Math.Max(confianzaBase, 0.05m);
            }

            return await Task.FromResult(confianzaBase);
        }

        public async Task<int> CalcularPuntajeAsync(PrediccionResponseDto prediccion)
        {
            if (prediccion.PuntosObtenidos.HasValue)
                return prediccion.PuntosObtenidos.Value;

            if (!prediccion.EsAcertada)
                return 0;

            int puntos = 3;

            if (prediccion.GolesRealesLocal.HasValue &&
                prediccion.GolesRealesVisitante.HasValue &&
                prediccion.GolesLocalPredichos == prediccion.GolesRealesLocal.Value &&
                prediccion.GolesVisitantePredichos == prediccion.GolesRealesVisitante.Value)
            {
                puntos += 5;
            }

            if (prediccion.Confianza >= 0.7m)
                puntos += 2;

            if (prediccion.Confianza < 0.3m)
                puntos -= 1;

            return Math.Max(puntos, 0);
        }

        public async Task ActualizarResultadosAsync(int partidoId, int golesLocal, int golesVisitante)
        {
            try
            {
                var partido = await _context.Partidos.FindAsync(partidoId);
                if (partido == null)
                    throw new ArgumentException($"Partido con ID {partidoId} no encontrado");

                partido.GolesLocal = golesLocal;
                partido.GolesVisitante = golesVisitante;
                partido.Finalizado = true;
                partido.Estado = "Finalizado";

                var predicciones = await _context.Predicciones
                    .Where(p => p.PartidoId == partidoId)
                    .ToListAsync();

                foreach (var prediccion in predicciones)
                {
                    var esAcertada = prediccion.GolesLocalPredichos == golesLocal &&
                                    prediccion.GolesVisitantePredichos == golesVisitante;

                    prediccion.EsAcertada = esAcertada;

                    if (esAcertada)
                    {
                        var dto = await MapToResponseDto(prediccion);
                        prediccion.PuntosObtenidos = await CalcularPuntajeAsync(dto);
                    }
                    else
                    {
                        prediccion.PuntosObtenidos = 0;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar resultados del partido {partidoId}");
                throw;
            }
        }

        private async Task<PrediccionResponseDto> MapToResponseDto(Prediccion prediccion)
        {
            return new PrediccionResponseDto
            {
                Id = prediccion.Id,
                PartidoId = prediccion.PartidoId,
                GolesLocalPredichos = prediccion.GolesLocalPredichos,
                GolesVisitantePredichos = prediccion.GolesVisitantePredichos,
                ProbabilidadLocal = prediccion.ProbabilidadLocal,
                ProbabilidadEmpate = prediccion.ProbabilidadEmpate,
                ProbabilidadVisitante = prediccion.ProbabilidadVisitante,
                Confianza = prediccion.Confianza,
                FechaPrediccion = prediccion.FechaPrediccion,
                EsAcertada = prediccion.EsAcertada,
                PuntosObtenidos = prediccion.PuntosObtenidos,
                Comentarios = prediccion.Comentarios,
                EquipoLocalNombre = prediccion.Partido?.Local,
                EquipoVisitanteNombre = prediccion.Partido?.Visitante,
                EstadoPartido = prediccion.Partido?.Estado,
                GolesRealesLocal = prediccion.Partido?.GolesLocal,
                GolesRealesVisitante = prediccion.Partido?.GolesVisitante
            };
        }
    }
}