using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations
{
    public class PartidoConfiguration : IEntityTypeConfiguration<Partido>
    {
        public void Configure(EntityTypeBuilder<Partido> builder)
        {
            builder.ToTable("Partidos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.FechaHora)
                .IsRequired();

            builder.Property(p => p.Estadio)
                .HasMaxLength(50);

            builder.Property(p => p.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Programado");

            builder.Property(p => p.Finalizado)
                .HasDefaultValue(false);

            // Índices para mejorar rendimiento
            builder.HasIndex(p => p.FechaHora);
            builder.HasIndex(p => p.Estado);
            builder.HasIndex(p => new { p.FechaHora, p.Estado });
        }
    }
}
