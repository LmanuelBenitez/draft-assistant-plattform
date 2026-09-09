using backend.DTOs.Request;
using backend.DTOs.Response;

namespace backend.Services.Interfaces
{
    public interface IPrediccionService
    {
        Task<PrediccionResponseDto> GenerarPrediccionAsync(PartidoRequestDto request);
        Task<PrediccionResponseDto> ObtenerPrediccionAsync(int id);
        Task<bool> ValidarPrediccionAsync(PrediccionResponseDto prediccion);
        Task<decimal> CalcularConfianzaAsync(PrediccionResponseDto prediccion);
        Task<int> CalcularPuntajeAsync(PrediccionResponseDto prediccion);
        Task ActualizarResultadosAsync(int partidoId, int golesLocal, int golesVisitante);
    }
}
