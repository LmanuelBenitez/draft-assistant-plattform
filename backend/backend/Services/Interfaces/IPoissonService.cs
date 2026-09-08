namespace backend.Services.Interfaces
{
    public interface IPoissonService
    {
        /// <summary>
        /// Calcula la probabilidad de que un equipo con cierta fuerza anotadora
        /// marque exactamente 'goles' goles usando la distribución de Poisson.
        /// </summary>
        /// <param name="lambda">Tasa esperada de goles</param>
        /// <param name="goles">Número de goles a evaluar</param>
        /// <returns>Probabilidad (0-1)</returns>
        double CalcularProbabilidadPoisson(double lambda, int goles);

        /// <summary>
        /// Calcula las probabilidades de resultado (local, empate, visitante)
        /// para un partido dados los promedios de goles de ambos equipos.
        /// </summary>
        Task<(double Local, double Empate, double Visitante)> CalcularProbabilidadesPartidoAsync(
            double promedioLocal,
            double promedioVisitante,
            int maxGoles = 10);

        /// <summary>
        /// Predice el marcador más probable dado los promedios de goles.
        /// </summary>
        Task<(int GolesLocal, int GolesVisitante)> PredecirMarcadorAsync(
            double promedioLocal,
            double promedioVisitante);

        /// <summary>
        /// Calcula la confianza de la predicción basada en la distribución.
        /// </summary>
        Task<double> CalcularConfianzaAsync(
            double promedioLocal,
            double promedioVisitante,
            int golesLocalPredichos,
            int golesVisitantePredichos);
    }
}
