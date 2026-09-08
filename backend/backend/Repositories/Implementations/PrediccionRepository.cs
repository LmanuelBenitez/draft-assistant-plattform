using backend.Models;
using backend.Repositories.Interfaces;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Implementations
{
    public class PrediccionRepository : IPrediccionRepository
    {
        private readonly AppDbContext _context;

        public PrediccionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Prediccion?> GetByIdAsync(int id)
        {
            return await _context.Predicciones
                .Include(p => p.Partido)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async IAsyncEnumerable<Prediccion> GetByUsuarioIdAsync(string usuarioId)
        {
            var query = _context.Predicciones
                .Include(p => p.Partido)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Prediccion> GetByPartidoIdAsync(int partidoId)
        {
            var query = _context.Predicciones
                .Include(p => p.Partido)
                .Where(p => p.PartidoId == partidoId)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Prediccion> GetByPartidoIdAndUsuarioIdAsync(int partidoId, string usuarioId)
        {
            var query = _context.Predicciones
                .Include(p => p.Partido)
                .Where(p => p.PartidoId == partidoId && p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Prediccion> GetPrediccionesAcertadasAsync()
        {
            var query = _context.Predicciones
                .Include(p => p.Partido)
                .Where(p => p.EsAcertada)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async Task<Prediccion> AddAsync(Prediccion prediccion)
        {
            _context.Predicciones.Add(prediccion);
            await _context.SaveChangesAsync();
            return prediccion;
        }

        public async Task<Prediccion> UpdateAsync(Prediccion prediccion)
        {
            _context.Entry(prediccion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return prediccion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var prediccion = await _context.Predicciones.FindAsync(id);
            if (prediccion == null)
                return false;

            _context.Predicciones.Remove(prediccion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async IAsyncEnumerable<Prediccion> GetPrediccionesWithDetailsAsync(int? partidoId = null)
        {
            var query = _context.Predicciones
                .Include(p => p.Partido)
                .AsQueryable();

            if (partidoId.HasValue)
                query = query.Where(p => p.PartidoId == partidoId.Value);

            var asyncQuery = query
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in asyncQuery)
            {
                yield return item;
            }
        }

        public async Task<int> CountByUsuarioIdAsync(string usuarioId)
        {
            return await _context.Predicciones
                .Where(p => p.UsuarioId == usuarioId)
                .CountAsync();
        }

        public async Task<int> CountAcertadasByUsuarioIdAsync(string usuarioId)
        {
            return await _context.Predicciones
                .Where(p => p.UsuarioId == usuarioId && p.EsAcertada)
                .CountAsync();
        }
    }
}
