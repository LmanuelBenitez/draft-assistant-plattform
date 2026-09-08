namespace backend.Services.Interfaces
{
    public interface IDeepSeekService
    {
        /// <summary>
        /// Envía una consulta al modelo DeepSeek y obtiene una respuesta.
        /// </summary>
        Task<string> ConsultarAsync(string prompt, int maxTokens = 500);

        /// <summary>
        /// Obtiene una predicción de fútbol detallada en formato JSON.
        /// </summary>
        Task<string> ObtenerPrediccionFutbolAsync(
            string equipoLocal,
            string equipoVisitante,
            string? estadio = null,
            string? contexto = null);

        /// <summary>
        /// Analiza el rendimiento de un equipo y devuelve métricas clave.
        /// </summary>
        Task<string> AnalizarEquipoAsync(string equipoNombre);

        /// <summary>
        /// Verifica si el servicio está disponible.
        /// </summary>
        Task<bool> VerificarDisponibilidadAsync();
    }
}
