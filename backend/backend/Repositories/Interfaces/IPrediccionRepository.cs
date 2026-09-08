using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IPrediccionRepository
    {
        Task<Prediccion?> GetByIdAsync(int id);
        IAsyncEnumerable<Prediccion> GetByUsuarioIdAsync(string usuarioId);
        IAsyncEnumerable<Prediccion> GetByPartidoIdAsync(int partidoId);
        IAsyncEnumerable<Prediccion> GetByPartidoIdAndUsuarioIdAsync(int partidoId, string usuarioId);
        IAsyncEnumerable<Prediccion> GetPrediccionesAcertadasAsync();
        Task<Prediccion> AddAsync(Prediccion prediccion);
        Task<Prediccion> UpdateAsync(Prediccion prediccion);
        Task<bool> DeleteAsync(int id);
        IAsyncEnumerable<Prediccion> GetPrediccionesWithDetailsAsync(int? partidoId = null);
        Task<int> CountByUsuarioIdAsync(string usuarioId);
        Task<int> CountAcertadasByUsuarioIdAsync(string usuarioId);
    }
}
