using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("Partidos")]
    public class Partido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Local { get; set; }

        [Required]
        public required string Visitante { get; set; }

        [Required]
        public required int? LigaId { get; set; }

        [Required]
        public required string Temporada { get; set; }

        public string? Bajas { get; set; }

        public string? Contexto { get; set; }

        public DateTime? FechaHora { get; set; }

        [MaxLength(50)]
        public string? Estadio { get; set; }

        public string? Competicion { get; set; }

        public int? GolesLocal { get; set; }

        public int? GolesVisitante { get; set; }

        public bool Finalizado { get; set; } = false;

        [MaxLength(20)]
        public string? Estado { get; set; } = "Programado";

        public virtual ICollection<Prediccion> Predicciones { get; set; } = new List<Prediccion>();
    }
}
