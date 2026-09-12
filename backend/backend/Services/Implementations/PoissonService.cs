using backend.Utils.Constants;
using backend.Utils.Helpers;
using backend.Services.Interfaces;

namespace backend.Services.Implementations
{
    public class PoissonService : IPoissonService
    {
        private static readonly double[] FactorialCache = new double[20];

        // Parámetro rho de Dixon-Coles
        private readonly double _rho;

        public PoissonService()
        {
            // Inicializar caché de factoriales
            FactorialCache[0] = 1;
            for (int i = 1; i < FactorialCache.Length; i++)
            {
                FactorialCache[i] = FactorialCache[i - 1] * i;
            }

            // ✅ Valor por defecto de rho
            _rho = DixonColesConstants.RhoPorDefecto;
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

        /// <summary>
        /// Calcula las probabilidades usando el modelo Dixon-Coles.
        /// Corrige la subestimación de empates de Poisson estándar.
        /// </summary>
        public async Task<(decimal Local, decimal Empate, decimal Visitante)> CalcularProbabilidadesPartidoAsync(
            double promedioLocal,
            double promedioVisitante,
            int maxGoles = 10)
        {
            if (promedioLocal < 0 || promedioVisitante < 0)
                throw new ArgumentException("Los promedios de goles deben ser mayores o iguales a 0");

            double probLocal = 0;
            double probEmpate = 0;
            double probVisitante = 0;
            double probTotal = 0;

            // Calcular probabilidad de cada marcador posible con Dixon-Coles
            for (int golesLocal = 0; golesLocal <= maxGoles; golesLocal++)
            {
                for (int golesVisitante = 0; golesVisitante <= maxGoles; golesVisitante++)
                {
                    // ✅ Usar Dixon-Coles en lugar de Poisson estándar
                    double prob = DixonColesHelper.CalcularProbabilidadMarcador(
                        golesLocal,
                        golesVisitante,
                        promedioLocal,
                        promedioVisitante,
                        _rho,
                        CalcularProbabilidadPoisson);

                    probTotal += prob;

                    if (golesLocal > golesVisitante)
                        probLocal += prob;
                    else if (golesLocal == golesVisitante)
                        probEmpate += prob;
                    else
                        probVisitante += prob;
                }
            }

            // ✅ Normalizar (Dixon-Coles puede no sumar 1.0 exactamente)
            if (probTotal > 0)
            {
                probLocal /= probTotal;
                probEmpate /= probTotal;
                probVisitante /= probTotal;
            }

            return await Task.FromResult((
                (decimal)probLocal,
                (decimal)probEmpate,
                (decimal)probVisitante
            ));
        }

        /// <summary>
        /// Predice el marcador más probable usando Dixon-Coles.
        /// </summary>
        public async Task<(int GolesLocal, int GolesVisitante)> PredecirMarcadorAsync(
            double promedioLocal,
            double promedioVisitante)
        {
            if (promedioLocal < 0 || promedioVisitante < 0)
                throw new ArgumentException("Los promedios de goles deben ser mayores o iguales a 0");

            double maxProb = 0;
            int mejorGolesLocal = 0;
            int mejorGolesVisitante = 0;

            // ✅ Buscar el marcador con mayor probabilidad según Dixon-Coles
            for (int golesLocal = 0; golesLocal <= DixonColesConstants.MaxGoles; golesLocal++)
            {
                for (int golesVisitante = 0; golesVisitante <= DixonColesConstants.MaxGoles; golesVisitante++)
                {
                    double prob = DixonColesHelper.CalcularProbabilidadMarcador(
                        golesLocal,
                        golesVisitante,
                        promedioLocal,
                        promedioVisitante,
                        _rho,
                        CalcularProbabilidadPoisson);

                    if (prob > maxProb)
                    {
                        maxProb = prob;
                        mejorGolesLocal = golesLocal;
                        mejorGolesVisitante = golesVisitante;
                    }
                }
            }

            return await Task.FromResult((mejorGolesLocal, mejorGolesVisitante));
        }

        /// <summary>
        /// Calcula la confianza basada en Dixon-Coles.
        /// </summary>
        public async Task<double> CalcularConfianzaAsync(
            double promedioLocal,
            double promedioVisitante,
            int golesLocalPredichos,
            int golesVisitantePredichos)
        {
            // ✅ Usar Dixon-Coles para calcular la probabilidad del marcador
            double probMarcador = DixonColesHelper.CalcularProbabilidadMarcador(
                golesLocalPredichos,
                golesVisitantePredichos,
                promedioLocal,
                promedioVisitante,
                _rho,
                CalcularProbabilidadPoisson);

            // Calcular la probabilidad total
            double probTotal = 0;
            for (int i = 0; i <= DixonColesConstants.MaxGoles; i++)
            {
                for (int j = 0; j <= DixonColesConstants.MaxGoles; j++)
                {
                    probTotal += DixonColesHelper.CalcularProbabilidadMarcador(
                        i, j, promedioLocal, promedioVisitante, _rho, CalcularProbabilidadPoisson);
                }
            }

            // Normalizar
            double confianza = probTotal > 0 ? probMarcador / probTotal : 0;

            // Escalar
            confianza = Math.Min(confianza * 10, 1.0);

            // ✅ Limitar confianza máxima a 85%
            confianza = Math.Min(confianza, 0.85);
            confianza = Math.Max(confianza, 0.30);

            return await Task.FromResult(Math.Round(confianza, 2));
        }
    }
}