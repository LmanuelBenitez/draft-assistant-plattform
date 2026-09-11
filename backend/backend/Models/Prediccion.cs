namespace backend.Models;

public class Prediccion
{
    public int Id { get; set; }

    // Equipos
    public string Local { get; set; } = string.Empty;
    public string Visitante { get; set; } = string.Empty;
    public int? LigaIdLocal { get; set; }
    public int? LigaIdVisitante { get; set; }
    public string? Temporada { get; set; }

    // Datos manuales (frontend)
    public string? Competicion { get; set; }
    public string? Estadio { get; set; }
    public string? Bajas { get; set; }
    public string? Contexto { get; set; }

    // Resultado Poisson
    public int GolesLocalPredichos { get; set; }
    public int GolesVisitantePredichos { get; set; }
    public decimal ProbabilidadLocal { get; set; }
    public decimal ProbabilidadEmpate { get; set; }
    public decimal ProbabilidadVisitante { get; set; }
    public decimal Confianza { get; set; }
    public decimal PromedioGolesLocal { get; set; }
    public decimal PromedioGolesVisitante { get; set; }

    // DeepSeek
    public string? AnalisisDeepSeek { get; set; }

    // Metadata
    public DateTime FechaPrediccion { get; set; } = DateTime.UtcNow;
    public bool EsAcertada { get; set; }
    public int? PuntosObtenidos { get; set; }
    public int? GolesRealesLocal { get; set; }
    public int? GolesRealesVisitante { get; set; }
}