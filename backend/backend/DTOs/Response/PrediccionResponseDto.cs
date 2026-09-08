namespace backend.DTOs.Response
{
    public record PrediccionResponseDto
    {
        public int Id { get; init; }

        // Equipos
        public string Local { get; init; } = string.Empty;
        public string Visitante { get; init; } = string.Empty;
        public int? LigaIdLocal { get; init; }
        public int? LigaIdVisitante { get; init; }
        public string? Temporada { get; init; }

        // Datos manuales (frontend)
        public string? Competicion { get; init; }
        public string? Estadio { get; init; }
        public string? Bajas { get; init; }
        public string? Contexto { get; init; }
        public string? Clima { get; init; }

        // Resultado Poisson
        public int GolesLocalPredichos { get; init; }
        public int GolesVisitantePredichos { get; init; }
        public decimal ProbabilidadLocal { get; init; }
        public decimal ProbabilidadEmpate { get; init; }
        public decimal ProbabilidadVisitante { get; init; }
        public decimal Confianza { get; init; }

        // Promedios usados
        public decimal PromedioGolesLocal { get; init; }
        public decimal PromedioGolesVisitante { get; init; }

        // DeepSeek
        public string? AnalisisDeepSeek { get; init; }

        // Metadata
        public DateTime FechaPrediccion { get; init; }
        public bool EsAcertada { get; init; }
        public int? PuntosObtenidos { get; init; }
        public int? GolesRealesLocal { get; init; }
        public int? GolesRealesVisitante { get; init; }
    }
}