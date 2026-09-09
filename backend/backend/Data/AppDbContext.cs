using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data.Configurations;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Prediccion> Predicciones { get; set; }
    public DbSet<Partido> Partidos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ Aplicar configuraciones
        modelBuilder.ApplyConfiguration(new PrediccionConfiguration());
        modelBuilder.ApplyConfiguration(new PartidoConfiguration());
    }
}