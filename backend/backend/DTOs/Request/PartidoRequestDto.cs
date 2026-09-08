using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Request
{
    public record PartidoRequestDto
    {
        [Required]
        public required string Local { get; init; }

        [Required]
        public required string Visitante { get; init; }

        [Required]
        public required DateTime FechaHora { get; init; }

        [MaxLength(50)]
        public string? Estadio { get; init; }

        public int? GolesLocal { get; init; }

        public int? GolesVisitante { get; init; }

        [MaxLength(20)]
        public string? Estado { get; init; }
    }
}
