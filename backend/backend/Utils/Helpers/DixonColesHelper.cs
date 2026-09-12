namespace backend.Utils.Helpers
{
    public static class DixonColesHelper
    {
        /// <summary>
        /// Calcula la función τ (tau) de Dixon-Coles.
        /// Corrige la dependencia entre goles del local y visitante.
        /// </summary>
        /// <param name="golesLocal">Goles del equipo local</param>
        /// <param name="golesVisitante">Goles del equipo visitante</param>
        /// <param name="lambda">Promedio de goles del local</param>
        /// <param name="mu">Promedio de goles del visitante</param>
        /// <param name="rho">Parámetro de dependencia</param>
        public static double CalcularTau(
            int golesLocal,
            int golesVisitante,
            double lambda,
            double mu,
            double rho)
        {
            // τ(0,0) = 1 - (λ * μ * ρ)
            if (golesLocal == 0 && golesVisitante == 0)
                return 1 - (lambda * mu * rho);

            // τ(0,1) = 1 + (λ * ρ)
            if (golesLocal == 0 && golesVisitante == 1)
                return 1 + (lambda * rho);

            // τ(1,0) = 1 + (μ * ρ)
            if (golesLocal == 1 && golesVisitante == 0)
                return 1 + (mu * rho);

            // τ(1,1) = 1 - ρ
            if (golesLocal == 1 && golesVisitante == 1)
                return 1 - rho;

            // Cualquier otro caso
            return 1.0;
        }

        /// <summary>
        /// Calcula la probabilidad de un marcador específico usando Dixon-Coles.
        /// </summary>
        public static double CalcularProbabilidadMarcador(
            int golesLocal,
            int golesVisitante,
            double lambda,
            double mu,
            double rho,
            Func<double, int, double> poissonFunc)
        {
            // Probabilidad con Poisson estándar
            var probPoisson = poissonFunc(lambda, golesLocal) * poissonFunc(mu, golesVisitante);

            // Factor de corrección Dixon-Coles
            var tau = CalcularTau(golesLocal, golesVisitante, lambda, mu, rho);

            // Probabilidad corregida
            var probCorregida = tau * probPoisson;

            // Evitar valores negativos (puede pasar si tau < 0 por rho mal configurado)
            return Math.Max(probCorregida, 0);
        }
    }
}
