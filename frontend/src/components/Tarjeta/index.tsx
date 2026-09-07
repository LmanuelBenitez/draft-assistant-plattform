import React from 'react';
import { RECOMENDACION_COLORS } from '../../constants';
import { formatearProbabilidad } from '../../utils/formatters';
import type { PrediccionResponse } from '../../types';

interface TarjetaProps {
  prediccion: PrediccionResponse;
  local: string;
  visitante: string;
}

const Tarjeta: React.FC<TarjetaProps> = ({ prediccion, local, visitante }) => {
  const { probabilidades, recomendacion, explicacion, factores_clave, alertas } = prediccion;

  const obtenerColorRecomendacion = (recom: string): string => {
    const lower = recom.toLowerCase();
    if (lower.includes('local') || lower.includes('casa')) {
      return RECOMENDACION_COLORS.local;
    } else if (lower.includes('empate')) {
      return RECOMENDACION_COLORS.empate;
    } else if (lower.includes('visitante') || lower.includes('fuera')) {
      return RECOMENDACION_COLORS.visitante;
    }
    return 'text-gray-600 dark:text-gray-300';
  };

  const obtenerEmojiRecomendacion = (recom: string): string => {
    const lower = recom.toLowerCase();
    if (lower.includes('local') || lower.includes('casa')) {
      return '🏠';
    } else if (lower.includes('empate')) {
      return '🤝';
    } else if (lower.includes('visitante') || lower.includes('fuera')) {
      return '✈️';
    }
    return '📊';
  };

  return (
    <div className="w-full max-w-2xl mx-auto p-6 bg-white dark:bg-gray-800 rounded-xl shadow-lg space-y-6">
      {/* Encabezado */}
      <div className="text-center border-b border-gray-200 dark:border-gray-700 pb-4">
        <h3 className="text-xl font-bold text-gray-900 dark:text-white">
          {local} vs {visitante}
        </h3>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Análisis completo del partido</p>
      </div>

      {/* Recomendación */}
      <div className="bg-purple-50 dark:bg-purple-900/20 rounded-lg p-4 border border-purple-200 dark:border-purple-800">
        <div className="flex items-center gap-3">
          <span className="text-3xl">{obtenerEmojiRecomendacion(recomendacion)}</span>
          <div>
            <p className="text-sm font-medium text-purple-700 dark:text-purple-300">
              Recomendación
            </p>
            <p className={`text-lg font-bold ${obtenerColorRecomendacion(recomendacion)}`}>
              {recomendacion}
            </p>
          </div>
        </div>
      </div>

      {/* Probabilidades resumidas */}
      <div className="grid grid-cols-3 gap-3">
        <div className="text-center p-3 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
          <p className="text-sm text-gray-600 dark:text-gray-400">Local</p>
          <p className="text-xl font-bold text-purple-600 dark:text-purple-400">
            {formatearProbabilidad(probabilidades.local)}
          </p>
        </div>
        <div className="text-center p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg">
          <p className="text-sm text-gray-600 dark:text-gray-400">Empate</p>
          <p className="text-xl font-bold text-yellow-600 dark:text-yellow-400">
            {formatearProbabilidad(probabilidades.empate)}
          </p>
        </div>
        <div className="text-center p-3 bg-red-50 dark:bg-red-900/20 rounded-lg">
          <p className="text-sm text-gray-600 dark:text-gray-400">Visitante</p>
          <p className="text-xl font-bold text-red-600 dark:text-red-400">
            {formatearProbabilidad(probabilidades.visitante)}
          </p>
        </div>
      </div>

      {/* Explicación */}
      <div>
        <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
          📝 Explicación
        </h4>
        <p className="text-gray-600 dark:text-gray-300 text-sm leading-relaxed">{explicacion}</p>
      </div>

      {/* Factores clave */}
      {factores_clave && factores_clave.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
            🔑 Factores Clave
          </h4>
          <ul className="list-disc list-inside space-y-1">
            {factores_clave.map((factor, index) => (
              <li key={index} className="text-gray-600 dark:text-gray-300 text-sm">
                {factor}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Alertas */}
      {alertas && alertas.length > 0 && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <h4 className="text-sm font-semibold text-red-700 dark:text-red-300 mb-2">
            ⚠️ Alertas
          </h4>
          <ul className="list-disc list-inside space-y-1">
            {alertas.map((alerta, index) => (
              <li key={index} className="text-red-600 dark:text-red-400 text-sm">
                {alerta}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default Tarjeta;
