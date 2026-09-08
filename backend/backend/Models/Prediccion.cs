using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("Predicciones")]
    public class Prediccion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required int PartidoId { get; set; }

        [Required]
        public required int GolesLocalPredichos { get; set; }

        [Required]
        public required int GolesVisitantePredichos { get; set; }

        public decimal? ProbabilidadLocal { get; set; }

        public decimal? ProbabilidadEmpate { get; set; }

        public decimal? ProbabilidadVisitante { get; set; }

        public decimal? Confianza { get; set; }

        public DateTime FechaPrediccion { get; set; } = DateTime.UtcNow;

        public bool EsAcertada { get; set; } = false;

        public int? PuntosObtenidos { get; set; }

        [MaxLength(500)]
        public string? Comentarios { get; set; }

        // Navigation property
        [ForeignKey("PartidoId")]
        public virtual Partido Partido { get; set; } = null!;
    }
}
