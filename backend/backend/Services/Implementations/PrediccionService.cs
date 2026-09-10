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

                // 2. Obtener estadísticas desde API-Football
                var statsLocal = await _footballApi.GetEstadisticasEquipoAsync(
                    request.Local, request.LigaId, request.Temporada);
                var statsVisitante = await _footballApi.GetEstadisticasEquipoAsync(
                    request.Visitante, request.LigaId, request.Temporada);
                var historicoH2H = await _footballApi.GetPartidosHead2HeadAsync(
                    statsLocal.EquipoId, statsVisitante.EquipoId, request.LigaId, request.Temporada);

                // 3. Calcular promedios base
                var promedioLocalBase = statsLocal.PromedioGolesFavor > 0 ? statsLocal.PromedioGolesFavor : 1.0;
                var promedioVisitanteBase = statsVisitante.PromedioGolesFavor > 0 ? statsVisitante.PromedioGolesFavor : 1.0;

                // 4. Ajustar promedios con Head-to-Head (si hay datos suficientes)
                var promedioLocal = promedioLocalBase;
                var promedioVisitante = promedioVisitanteBase;

                if (historicoH2H != null && historicoH2H.Count >= 3)
                {
                    // Calcular promedios en enfrentamientos directos
                    var golesLocalH2H = historicoH2H
                        .Where(p => p.Local == request.Local || p.Visitante == request.Local)
                        .Select(p => p.Local == request.Local ? p.GolesLocal : p.GolesVisitante)
                        .DefaultIfEmpty(0)
                        .Average();

                    var golesVisitanteH2H = historicoH2H
                        .Where(p => p.Local == request.Visitante || p.Visitante == request.Visitante)
                        .Select(p => p.Local == request.Visitante ? p.GolesLocal : p.GolesVisitante)
                        .DefaultIfEmpty(0)
                        .Average();

                    // Si hay datos válidos, ajustar con peso 70% general / 30% H2H
                    if (golesLocalH2H > 0) promedioLocal = (promedioLocalBase * 0.7) + (golesLocalH2H * 0.3);
                    if (golesVisitanteH2H > 0) promedioVisitante = (promedioVisitanteBase * 0.7) + (golesVisitanteH2H * 0.3);

                    _logger.LogInformation("Promedios ajustados con H2H: {Local} {PromLocal:F2} (base {BaseLocal:F2}), {Visitante} {PromVisit:F2} (base {BaseVisit:F2})",
                        request.Local, promedioLocal, promedioLocalBase,
                        request.Visitante, promedioVisitante, promedioVisitanteBase);
                }

                // 5. Calcular predicción con Poisson
                var marcador = await _poissonService.PredecirMarcadorAsync(promedioLocal, promedioVisitante);
                var probabilidades = await _poissonService.CalcularProbabilidadesPartidoAsync(promedioLocal, promedioVisitante);
                var confianza = await _poissonService.CalcularConfianzaAsync(
                    promedioLocal,
                    promedioVisitante,
                    marcador.GolesLocal,
                    marcador.GolesVisitante);

                // 6. Ajustar confianza según historial H2H
                if (historicoH2H != null && historicoH2H.Count >= 3)
                {
                    var victoriasLocal = historicoH2H.Count(p => p.GolesLocal > p.GolesVisitante);
                    var victoriasVisitante = historicoH2H.Count(p => p.GolesVisitante > p.GolesLocal);
                    var diferencia = Math.Abs(victoriasLocal - victoriasVisitante);

                    // Si un equipo domina claramente el H2H, aumentar confianza
                    if (diferencia >= 3)
                    {
                        var factor = 1.0 + (diferencia * 0.02);
                        confianza = Math.Min(confianza * factor, 0.95);
                    }
                }

                // 7. Guardar la predicción en BD (sin Partido)
                var prediccion = new Prediccion
                {
                    Local = request.Local,
                    Visitante = request.Visitante,
                    LigaIdLocal = int.TryParse(request.LigaId, out var ligaLocal) ? ligaLocal : (int?)null,
                    LigaIdVisitante = int.TryParse(request.LigaId, out var ligaVisitante) ? ligaVisitante : (int?)null,
                    Temporada = request.Temporada,
                    Competicion = request.Competicion,
                    Estadio = request.Estadio,
                    Bajas = request.Bajas,
                    Contexto = request.Contexto,
                    GolesLocalPredichos = marcador.GolesLocal,
                    GolesVisitantePredichos = marcador.GolesVisitante,
                    ProbabilidadLocal = probabilidades.Local,
                    ProbabilidadEmpate = probabilidades.Empate,
                    ProbabilidadVisitante = probabilidades.Visitante,
                    Confianza = (decimal)confianza,
                    PromedioGolesLocal = (decimal)promedioLocal,
                    PromedioGolesVisitante = (decimal)promedioVisitante,
                    FechaPrediccion = DateTime.UtcNow
                };

                _context.Predicciones.Add(prediccion);
                await _context.SaveChangesAsync();

                // 7.5. Guardar el partido en BD (si no existe)
                var partido = new Partido
                {
                    Local = request.Local,
                    Visitante = request.Visitante,
                    LigaId = int.TryParse(request.LigaId, out var liga) ? liga : (int?)null,
                    Temporada = request.Temporada,
                    Competicion = request.Competicion,
                    Estadio = request.Estadio,
                    Bajas = request.Bajas,
                    Contexto = request.Contexto,
                    FechaHora = request.FechaHora,
                    GolesLocal = request.GolesLocal,
                    GolesVisitante = request.GolesVisitante,
                    Estado = request.Estado
                };

                _context.Partidos.Add(partido);
                await _context.SaveChangesAsync();

                // 8. Obtener análisis de DeepSeek (con TODOS los datos)
                try
                {
                    var deepSeekResponse = await _deepSeekService.ObtenerPrediccionFutbolAsync(
                        equipoLocal: request.Local,
                        equipoVisitante: request.Visitante,
                        promedioGolesLocal: promedioLocal,
                        promedioGolesVisitante: promedioVisitante,
                        probLocal: probabilidades.Local,
                        probEmpate: probabilidades.Empate,
                        probVisitante: probabilidades.Visitante,
                        competicion: request.Competicion,
                        estadio: request.Estadio,
                        bajas: request.Bajas,
                        contexto: request.Contexto,
                        historicoEnfrentamientos: historicoH2H
                    );

                    prediccion.AnalisisDeepSeek = deepSeekResponse;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al obtener predicción de DeepSeek");
                    prediccion.AnalisisDeepSeek = "No se pudo obtener el análisis de DeepSeek";
                }

                // 9. Devolver respuesta
                return MapToResponseDto(prediccion);
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
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prediccion == null)
                throw new ArgumentException($"Predicción con ID {id} no encontrada");

            return MapToResponseDto(prediccion);
        }

        public async IAsyncEnumerable<PrediccionResponseDto> ObtenerPrediccionesPorEquipoAsync(string equipo)
        {
            await foreach (var p in _context.Predicciones
                .Where(p => p.Local == equipo || p.Visitante == equipo)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable())
            {
                yield return MapToResponseDto(p);
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

            var suma = prediccion.ProbabilidadLocal +
                       prediccion.ProbabilidadEmpate +
                       prediccion.ProbabilidadVisitante;

            return Math.Abs(suma - 1) < 0.01m;
        }

        public async Task<decimal> CalcularConfianzaAsync(PrediccionResponseDto prediccion)
        {
            var confianzaBase = prediccion.Confianza;

            var maxProb = Math.Max(
                Math.Max(prediccion.ProbabilidadLocal, prediccion.ProbabilidadVisitante),
                prediccion.ProbabilidadEmpate);

            var ajuste = (maxProb - 0.33m) * 1.5m   ;
            confianzaBase = Math.Min(confianzaBase + ajuste, 0.95m);
            confianzaBase = Math.Max(confianzaBase, 0.05m);

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

        public async Task ActualizarResultadosAsync(int prediccionId, int golesLocal, int golesVisitante)
        {
            try
            {
                var prediccion = await _context.Predicciones.FindAsync(prediccionId);

                if (prediccion == null)
                    throw new ArgumentException($"Predicción con ID {prediccionId} no encontrada");

                prediccion.GolesRealesLocal = golesLocal;
                prediccion.GolesRealesVisitante = golesVisitante;

                var esAcertada = prediccion.GolesLocalPredichos == golesLocal &&
                                prediccion.GolesVisitantePredichos == golesVisitante;

                prediccion.EsAcertada = esAcertada;

                if (esAcertada)
                {
                    var dto = MapToResponseDto(prediccion);
                    prediccion.PuntosObtenidos = await CalcularPuntajeAsync(dto);
                }
                else
                {
                    prediccion.PuntosObtenidos = 0;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Resultados actualizados para predicción {PrediccionId}: {Local} {GolesLocal} - {GolesVisitante} {Visitante}",
                    prediccionId, prediccion.Local, golesLocal, golesVisitante, prediccion.Visitante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar resultados de la predicción {prediccionId}");
                throw;
            }
        }

        private PrediccionResponseDto MapToResponseDto(Prediccion prediccion)
        {
            return new PrediccionResponseDto
            {
                Id = prediccion.Id,
                Local = prediccion.Local,
                Visitante = prediccion.Visitante,
                LigaIdLocal = prediccion.LigaIdLocal,
                LigaIdVisitante = prediccion.LigaIdVisitante,
                Temporada = prediccion.Temporada,
                Competicion = prediccion.Competicion,
                Estadio = prediccion.Estadio,
                Bajas = prediccion.Bajas,
                Contexto = prediccion.Contexto,
                GolesLocalPredichos = prediccion.GolesLocalPredichos,
                GolesVisitantePredichos = prediccion.GolesVisitantePredichos,
                ProbabilidadLocal = prediccion.ProbabilidadLocal,
                ProbabilidadEmpate = prediccion.ProbabilidadEmpate,
                ProbabilidadVisitante = prediccion.ProbabilidadVisitante,
                Confianza = prediccion.Confianza,
                PromedioGolesLocal = prediccion.PromedioGolesLocal,
                PromedioGolesVisitante = prediccion.PromedioGolesVisitante,
                AnalisisDeepSeek = prediccion.AnalisisDeepSeek,
                FechaPrediccion = prediccion.FechaPrediccion,
                EsAcertada = prediccion.EsAcertada,
                PuntosObtenidos = prediccion.PuntosObtenidos,
                GolesRealesLocal = prediccion.GolesRealesLocal,
                GolesRealesVisitante = prediccion.GolesRealesVisitante
            };
        }
    }
}