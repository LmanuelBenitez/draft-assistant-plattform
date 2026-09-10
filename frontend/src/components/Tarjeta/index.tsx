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
  const { 
    probabilidadLocal,
    probabilidadEmpate,
    probabilidadVisitante,
    analisisDeepSeek,
    confianza,
    golesLocalPredichos,
    golesVisitantePredichos,
    promedioGolesLocal,
    promedioGolesVisitante,
    competicion,
    estadio,
    temporada
  } = prediccion;

  // Construir objeto de probabilidades para mantener compatibilidad
  const probabilidades = {
    local: probabilidadLocal,
    empate: probabilidadEmpate,
    visitante: probabilidadVisitante
  };

  // Determinar recomendación basada en la probabilidad más alta
  const recomendacion = (() => {
    if (probabilidadLocal >= probabilidadEmpate && probabilidadLocal >= probabilidadVisitante) {
      return `Victoria de ${local}`;
    } else if (probabilidadEmpate >= probabilidadLocal && probabilidadEmpate >= probabilidadVisitante) {
      return 'Empate';
    } else {
      return `Victoria de ${visitante}`;
    }
  })();

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
        <div className="flex flex-wrap justify-center gap-2 mt-2 text-sm text-gray-500 dark:text-gray-400">
          {competicion && <span>🏆 {competicion}</span>}
          {temporada && <span>📅 {temporada}</span>}
          {estadio && <span>🏟️ {estadio}</span>}
        </div>
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
          {golesLocalPredichos !== undefined && (
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              ⚽ {golesLocalPredichos} goles
            </p>
          )}
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
          {golesVisitantePredichos !== undefined && (
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
              ⚽ {golesVisitantePredichos} goles
            </p>
          )}
        </div>
      </div>

      {/* Confianza y promedio de goles */}
      <div className="grid grid-cols-2 gap-3">
        <div className="text-center p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
          <p className="text-sm text-gray-600 dark:text-gray-400">Confianza</p>
          <p className="text-xl font-bold text-blue-600 dark:text-blue-400">
            {formatearProbabilidad(confianza)}
          </p>
        </div>
        <div className="text-center p-3 bg-green-50 dark:bg-green-900/20 rounded-lg">
          <p className="text-sm text-gray-600 dark:text-gray-400">Goles promedio</p>
          <p className="text-xl font-bold text-green-600 dark:text-green-400">
            {promedioGolesLocal?.toFixed(1)} / {promedioGolesVisitante?.toFixed(1)}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400">Local / Visitante</p>
        </div>
      </div>

      {/* Análisis */}
      <div>
        <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
          🤖 Análisis DeepSeek
        </h4>
        <p className="text-gray-600 dark:text-gray-300 text-sm leading-relaxed">{analisisDeepSeek}</p>
      </div>
    </div>
  );
};

export default Tarjeta;
