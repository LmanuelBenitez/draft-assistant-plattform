using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/prediccion")]
    public class PrediccionController(
        IPrediccionService prediccionService,
        ILogger<PrediccionController> logger) : ControllerBase
    {
        private readonly IPrediccionService _prediccionService = prediccionService;
        private readonly ILogger<PrediccionController> _logger = logger;

        /// <summary>
        /// Genera una predicción para un partido específico
        /// </summary>
        [HttpPost("generar")]
        [ProducesResponseType(typeof(PrediccionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrediccionResponseDto>> GenerarPrediccion(PartidoRequestDto request)
        {
            try
            {
                var resultado = await _prediccionService.GenerarPrediccionAsync(request);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar predicción para partido");
                return StatusCode(500, "Error interno al generar la predicción");
            }
        }

        /// <summary>
        /// Obtener todas las predicciones generadas
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<PrediccionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrediccionResponseDto>>> ObtenerPredicciones()
        {
            try
            {
                var resultado = await _prediccionService.ObtenerPrediccionesAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener predicciones");
                return StatusCode(500, "Error interno al obtener las predicciones");
            }
        }


        /// <summary>
        /// Obtiene una predicción por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrediccionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrediccionResponseDto>> ObtenerPrediccion(int id)
        {
            try
            {
                var resultado = await _prediccionService.ObtenerPrediccionAsync(id);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener predicción {id}");
                return StatusCode(500, "Error interno al obtener la predicción");
            }
        }

        /// <summary>
        /// Obtiene un analisis de Deepseek de una predicción por su ID
        /// </summary>
        [HttpGet("{id}/analisis")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> ObtenerAnalisisPrediccion(int id)
        {
            try
            {
                var resultado = await _prediccionService.ObtenerPrediccionAsync(id);
                return Ok(new
                {
                    id = resultado.Id,
                    analisisDeepseek = resultado.AnalisisDeepSeek,
                    listo = !string.IsNullOrEmpty(resultado.AnalisisDeepSeek)
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener analisis de Deepseek en la predicción {id}");
                return StatusCode(500, "Error interno al obtener el análisis de Deepseek");
            }
        }

        /// <summary>
        /// Actualiza los resultados de un partido y recalcula los puntajes
        /// </summary>
        [HttpPut("resultados/{partidoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarResultados(
            int partidoId,
            [FromBody] ActualizarResultadosRequest request)
        {
            try
            {
                if (request.GolesLocal < 0 || request.GolesVisitante < 0)
                    return BadRequest("Los goles no pueden ser negativos");

                await _prediccionService.ActualizarResultadosAsync(
                    partidoId,
                    request.GolesLocal,
                    request.GolesVisitante);

                return Ok(new { message = "Resultados actualizados correctamente" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar resultados del partido {partidoId}");
                return StatusCode(500, "Error interno al actualizar los resultados");
            }
        }

        /// <summary>
        /// Valida una predicción
        /// </summary>
        [HttpPost("validar")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ValidarPrediccion([FromBody] PrediccionResponseDto prediccion)
        {
            try
            {
                var esValida = await _prediccionService.ValidarPrediccionAsync(prediccion);
                return Ok(esValida);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar predicción");
                return StatusCode(500, "Error interno al validar la predicción");
            }
        }

        /// <summary>
        /// Calcula el puntaje de una predicción
        /// </summary>
        [HttpPost("puntaje")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> CalcularPuntaje([FromBody] PrediccionResponseDto prediccion)
        {
            try
            {
                var puntaje = await _prediccionService.CalcularPuntajeAsync(prediccion);
                return Ok(puntaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular puntaje");
                return StatusCode(500, "Error interno al calcular el puntaje");
            }
        }
    }

    public record ActualizarResultadosRequest
    {
        public int GolesLocal { get; init; }
        public int GolesVisitante { get; init; }
    }
}
