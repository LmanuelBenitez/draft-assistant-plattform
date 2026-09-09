using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IPrediccionRepository
    {
        // Obtener por ID
        Task<Prediccion?> GetByIdAsync(int id);

        // Obtener por equipo (local o visitante)
        IAsyncEnumerable<Prediccion> GetByLocalAsync(string local);
        IAsyncEnumerable<Prediccion> GetByVisitanteAsync(string visitante);
        IAsyncEnumerable<Prediccion> GetByEquipoAsync(string equipo);

        // Obtener predicciones acertadas
        IAsyncEnumerable<Prediccion> GetPrediccionesAcertadasAsync();

        // CRUD básico
        Task<Prediccion> AddAsync(Prediccion prediccion);
        Task<Prediccion> UpdateAsync(Prediccion prediccion);
        Task<bool> DeleteAsync(int id);

        // Obtener todas con detalles
        IAsyncEnumerable<Prediccion> GetPrediccionesWithDetailsAsync();

        // Contadores
        Task<int> CountByLocalAsync(string local);
        Task<int> CountByVisitanteAsync(string visitante);
        Task<int> CountByEquipoAsync(string equipo);
        Task<int> CountAcertadasByLocalAsync(string local);
        Task<int> CountAcertadasByVisitanteAsync(string visitante);
    }
}