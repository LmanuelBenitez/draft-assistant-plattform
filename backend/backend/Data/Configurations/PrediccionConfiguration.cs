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

            // ✅ Eliminar PartidoId (no existe en el modelo)
            // builder.Property(p => p.PartidoId).IsRequired();

            builder.Property(p => p.Local)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Visitante)
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

            // Índices para mejorar rendimiento
            builder.HasIndex(p => p.FechaPrediccion);
            builder.HasIndex(p => p.EsAcertada);
            builder.HasIndex(p => p.Local);
            builder.HasIndex(p => p.Visitante);
        }
    }
}