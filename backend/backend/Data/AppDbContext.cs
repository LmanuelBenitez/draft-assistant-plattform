using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Partido> Partidos { get; set; }
        public DbSet<Prediccion> Predicciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones adicionales
            modelBuilder.Entity<Partido>(entity =>
            {
                entity.HasIndex(p => p.FechaHora);
                entity.HasIndex(p => p.Estado);
            });

            modelBuilder.Entity<Prediccion>(entity =>
            {
                entity.HasOne(p => p.Partido)
                    .WithMany(p => p.Predicciones)
                    .HasForeignKey(p => p.PartidoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.FechaPrediccion);
                entity.HasIndex(p => p.EsAcertada);
            });


        }
    }
}
