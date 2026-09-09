using backend.Services.Interfaces;

namespace backend.Services.Implementations
{
    public class PoissonService : IPoissonService
    {
        private static readonly double[] FactorialCache = new double[20];

        static PoissonService()
        {
            FactorialCache[0] = 1;
            for (int i = 1; i < FactorialCache.Length; i++)
            {
                FactorialCache[i] = FactorialCache[i - 1] * i;
            }
        }

        public double CalcularProbabilidadPoisson(double lambda, int goles)
        {
            if (lambda < 0)
                throw new ArgumentException("Lambda debe ser mayor o igual a 0", nameof(lambda));

            if (goles < 0)
                throw new ArgumentException("El número de goles debe ser mayor o igual a 0", nameof(goles));

            if (lambda == 0 && goles == 0)
                return 1.0;

            if (lambda == 0)
                return 0.0;

            // P(X = k) = (e^(-lambda) * lambda^k) / k!
            double exponencial = Math.Exp(-lambda);
            double potencia = Math.Pow(lambda, goles);
            double factorial = ObtenerFactorial(goles);

            return (exponencial * potencia) / factorial;
        }

        private double ObtenerFactorial(int n)
        {
            if (n < FactorialCache.Length)
                return FactorialCache[n];

            double result = FactorialCache[FactorialCache.Length - 1];
            for (int i = FactorialCache.Length; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        public async Task<(decimal Local, decimal Empate, decimal Visitante)> CalcularProbabilidadesPartidoAsync(
            double promedioLocal,
            double promedioVisitante,
            int maxGoles = 10)
        {
            if (promedioLocal < 0 || promedioVisitante < 0)
                throw new ArgumentException("Los promedios de goles deben ser mayores o iguales a 0");

            decimal probLocal = 0;
            decimal probEmpate = 0;
            decimal probVisitante = 0;

            // Calcular probabilidad de cada marcador posible
            for (int golesLocal = 0; golesLocal <= maxGoles; golesLocal++)
            {
                for (int golesVisitante = 0; golesVisitante <= maxGoles; golesVisitante++)
                {
                    double prob = CalcularProbabilidadPoisson(promedioLocal, golesLocal) *
                                  CalcularProbabilidadPoisson(promedioVisitante, golesVisitante);

                    if (golesLocal > golesVisitante)
                        probLocal += (decimal)prob;
                    else if (golesLocal == golesVisitante)
                        probEmpate += (decimal)prob;
                    else
                        probVisitante += (decimal)prob;
                }
            }

            return await Task.FromResult((probLocal, probEmpate, probVisitante));
        }

        public async Task<(int GolesLocal, int GolesVisitante)> PredecirMarcadorAsync(
            double promedioLocal,
            double promedioVisitante)
        {
            if (promedioLocal < 0 || promedioVisitante < 0)
                throw new ArgumentException("Los promedios de goles deben ser mayores o iguales a 0");

            int golesLocal = EncontrarModa(promedioLocal, 10);
            int golesVisitante = EncontrarModa(promedioVisitante, 10);

            return await Task.FromResult((golesLocal, golesVisitante));
        }

        private int EncontrarModa(double lambda, int maxGoles)
        {
            double maxProb = 0;
            int moda = 0;

            for (int goles = 0; goles <= maxGoles; goles++)
            {
                double prob = CalcularProbabilidadPoisson(lambda, goles);
                if (prob > maxProb)
                {
                    maxProb = prob;
                    moda = goles;
                }
            }

            return moda;
        }

        public async Task<double> CalcularConfianzaAsync(
            double promedioLocal,
            double promedioVisitante,
            int golesLocalPredichos,
            int golesVisitantePredichos)
        {
            // Calcular la probabilidad del marcador específico
            double probMarcador = CalcularProbabilidadPoisson(promedioLocal, golesLocalPredichos) *
                                  CalcularProbabilidadPoisson(promedioVisitante, golesVisitantePredichos);

            // Calcular la probabilidad total de todos los marcadores
            double probTotal = 0;
            for (int i = 0; i <= 10; i++)
            {
                for (int j = 0; j <= 10; j++)
                {
                    probTotal += CalcularProbabilidadPoisson(promedioLocal, i) *
                                 CalcularProbabilidadPoisson(promedioVisitante, j);
                }
            }

            // Normalizar la confianza (0-1)
            double confianza = probTotal > 0 ? probMarcador / probTotal : 0;

            // Escalar para tener una medida más significativa
            confianza = Math.Min(confianza * 10, 1.0);

            return await Task.FromResult(confianza);
        }
    }
}
