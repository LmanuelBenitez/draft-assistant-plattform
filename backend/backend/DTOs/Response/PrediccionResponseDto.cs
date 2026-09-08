namespace backend.DTOs.Response
{
    public record PrediccionResponseDto
    {
        public int Id { get; init; }
        public int PartidoId { get; init; }
        public string? UsuarioId { get; init; }
        public int GolesLocalPredichos { get; init; }
        public int GolesVisitantePredichos { get; init; }
        public decimal? ProbabilidadLocal { get; init; }
        public decimal? ProbabilidadEmpate { get; init; }
        public decimal? ProbabilidadVisitante { get; init; }
        public decimal? Confianza { get; init; }
        public DateTime FechaPrediccion { get; init; }
        public bool EsAcertada { get; init; }
        public int? PuntosObtenidos { get; init; }
        public string? Comentarios { get; init; }
        public string? EquipoLocalNombre { get; init; }
        public string? EquipoVisitanteNombre { get; init; }
        public string? EstadoPartido { get; init; }
        public int? GolesRealesLocal { get; init; }
        public int? GolesRealesVisitante { get; init; }
    }
}
