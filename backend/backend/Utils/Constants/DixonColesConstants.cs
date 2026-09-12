namespace backend.Utils.Constants
{
    public static class DixonColesConstants
    {
        /// <summary>
        /// Valor típico de rho para fútbol profesional.
        /// Valores entre -0.1 y 0.1 son comunes.
        /// Basado en Dixon & Coles (1997).
        /// </summary>
        public const double RhoPorDefecto = -0.05;

        /// <summary>
        /// Máximo de goles a considerar en los cálculos
        /// </summary>
        public const int MaxGoles = 10;
    }
}
