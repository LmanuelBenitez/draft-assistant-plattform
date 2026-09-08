namespace backend.Helpers
{
    public static class PoissonHelper
    {
        /// <summary>
        /// Calcula la probabilidad de Poisson para un valor dado
        /// </summary>
        public static double CalculatePoissonProbability(double lambda, int k)
        {
            if (lambda < 0)
                throw new ArgumentException($"{nameof(lambda)} must be non-negative", nameof(lambda));

            if (k < 0)
                throw new ArgumentException($"{nameof(k)} must be non-negative", nameof(k));

            if (lambda == 0 && k == 0)
                return 1.0;

            if (lambda == 0)
                return 0.0;

            // Using natural log to avoid overflow
            double logProb = -lambda + k * Math.Log(lambda) - LogFactorial(k);
            return Math.Exp(logProb);
        }

        /// <summary>
        /// Calcula el logaritmo natural del factorial de n
        /// </summary>
        public static double LogFactorial(int n)
        {
            if (n < 0)
                throw new ArgumentException($"{nameof(n)} must be non-negative", nameof(n));

            if (n <= 1)
                return 0;

            // Stirling's approximation for large n
            if (n > 20)
                return n * Math.Log(n) - n + 0.5 * Math.Log(2 * Math.PI * n);

            // Direct calculation for small n
            double result = 0;
            for (int i = 2; i <= n; i++)
            {
                result += Math.Log(i);
            }
            return result;
        }

        /// <summary>
        /// Calcula el factorial de n
        /// </summary>
        public static long Factorial(int n)
        {
            if (n < 0)
                throw new ArgumentException($"{nameof(n)} must be non-negative", nameof(n));

            if (n > 20)
                throw new ArgumentException($"{nameof(n)} is too large for exact factorial", nameof(n));

            long result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        /// <summary>
        /// Encuentra la moda de una distribución de Poisson
        /// </summary>
        public static int FindPoissonMode(double lambda)
        {
            if (lambda < 0)
                throw new ArgumentException($"{nameof(lambda)} must be non-negative", nameof(lambda));

            if (lambda <= 1)
                return 0;

            return (int)Math.Floor(lambda);
        }

        /// <summary>
        /// Calcula la probabilidad acumulada de Poisson hasta k
        /// </summary>
        public static double CalculateCumulativePoisson(double lambda, int k)
        {
            if (lambda < 0)
                throw new ArgumentException($"{nameof(lambda)} must be non-negative", nameof(lambda));

            if (k < 0)
                throw new ArgumentException($"{nameof(k)} must be non-negative", nameof(k));

            double cumulative = 0;
            for (int i = 0; i <= k; i++)
            {
                cumulative += CalculatePoissonProbability(lambda, i);
            }
            return Math.Min(cumulative, 1.0);
        }
    }
}
