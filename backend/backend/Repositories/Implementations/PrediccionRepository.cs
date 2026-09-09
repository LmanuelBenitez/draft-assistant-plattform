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
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async IAsyncEnumerable<Prediccion> GetByLocalAsync(string local)
        {
            var query = _context.Predicciones
                .Where(p => p.Local == local)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Prediccion> GetByVisitanteAsync(string visitante)
        {
            var query = _context.Predicciones
                .Where(p => p.Visitante == visitante)
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Prediccion> GetByEquipoAsync(string equipo)
        {
            var query = _context.Predicciones
                .Where(p => p.Local == equipo || p.Visitante == equipo)
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

        public async IAsyncEnumerable<Prediccion> GetPrediccionesWithDetailsAsync()
        {
            var query = _context.Predicciones
                .OrderByDescending(p => p.FechaPrediccion)
                .AsAsyncEnumerable();

            await foreach (var item in query)
            {
                yield return item;
            }
        }

        public async Task<int> CountByLocalAsync(string local)
        {
            return await _context.Predicciones
                .Where(p => p.Local == local)
                .CountAsync();
        }

        public async Task<int> CountByVisitanteAsync(string visitante)
        {
            return await _context.Predicciones
                .Where(p => p.Visitante == visitante)
                .CountAsync();
        }

        public async Task<int> CountByEquipoAsync(string equipo)
        {
            return await _context.Predicciones
                .Where(p => p.Local == equipo || p.Visitante == equipo)
                .CountAsync();
        }

        public async Task<int> CountAcertadasByLocalAsync(string local)
        {
            return await _context.Predicciones
                .Where(p => p.Local == local && p.EsAcertada)
                .CountAsync();
        }

        public async Task<int> CountAcertadasByVisitanteAsync(string visitante)
        {
            return await _context.Predicciones
                .Where(p => p.Visitante == visitante && p.EsAcertada)
                .CountAsync();
        }
    }
}