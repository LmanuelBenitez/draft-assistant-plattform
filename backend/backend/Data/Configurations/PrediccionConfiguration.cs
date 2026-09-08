using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations
{
    public class PrediccionConfiguration : IEntityTypeConfiguration<Prediccion>
    {
        public void Configure(EntityTypeBuilder<Prediccion> builder)
        {
            builder.ToTable("Predicciones");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PartidoId)
                .IsRequired();

            builder.Property(p => p.UsuarioId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.GolesLocalPredichos)
                .IsRequired();

            builder.Property(p => p.GolesVisitantePredichos)
                .IsRequired();

            builder.Property(p => p.ProbabilidadLocal)
                .HasPrecision(18, 4);

            builder.Property(p => p.ProbabilidadEmpate)
                .HasPrecision(18, 4);

            builder.Property(p => p.ProbabilidadVisitante)
                .HasPrecision(18, 4);

            builder.Property(p => p.Confianza)
                .HasPrecision(18, 4);

            builder.Property(p => p.FechaPrediccion)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.EsAcertada)
                .HasDefaultValue(false);

            builder.Property(p => p.Comentarios)
                .HasMaxLength(500);

            // Relaciones
            builder.HasOne(p => p.Partido)
                .WithMany(p => p.Predicciones)
                .HasForeignKey(p => p.PartidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para mejorar rendimiento
            builder.HasIndex(p => p.UsuarioId);
            builder.HasIndex(p => p.FechaPrediccion);
            builder.HasIndex(p => p.EsAcertada);
            builder.HasIndex(p => new { p.PartidoId, p.UsuarioId });
        }
    }
}
