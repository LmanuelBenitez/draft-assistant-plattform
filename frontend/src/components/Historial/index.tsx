import React, { useState } from 'react';
import { TEXTOS } from '../../constants';
import { formatFecha, formatearProbabilidad, capitalizar } from '../../utils/formatters';
import type { PrediccionHistorial } from '../../types';

interface HistorialProps {
  predicciones: PrediccionHistorial[];
  onClear: () => void;
}

const Historial: React.FC<HistorialProps> = ({ predicciones, onClear }) => {
  const [showConfirm, setShowConfirm] = useState(false);

  const handleClearClick = () => {
    setShowConfirm(true);
  };

  const handleConfirmClear = () => {
    onClear();
    setShowConfirm(false);
  };

  const handleCancelClear = () => {
    setShowConfirm(false);
  };

  if (predicciones.length === 0) {
    return (
      <div className="w-full max-w-2xl mx-auto p-6 bg-white dark:bg-gray-800 rounded-xl shadow-lg">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-semibold text-gray-800 dark:text-white">
            📋 Historial de Predicciones
          </h3>
        </div>
        <p className="text-center text-gray-500 dark:text-gray-400 py-8">
          No hay predicciones en el historial
        </p>
      </div>
    );
  }

  const getRecomendacionColor = (recomendacion: string): string => {
    const lower = recomendacion.toLowerCase();
    if (lower.includes('local') || lower.includes('casa')) {
      return 'text-purple-600 dark:text-purple-400';
    } else if (lower.includes('empate')) {
      return 'text-yellow-600 dark:text-yellow-400';
    } else if (lower.includes('visitante') || lower.includes('fuera')) {
      return 'text-red-600 dark:text-red-400';
    }
    return 'text-gray-600 dark:text-gray-300';
  };

  return (
    <div className="w-full max-w-2xl mx-auto p-6 bg-white dark:bg-gray-800 rounded-xl shadow-lg">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold text-gray-800 dark:text-white">
          📋 Historial de Predicciones
          <span className="ml-2 text-sm font-normal text-gray-500 dark:text-gray-400">
            ({predicciones.length})
          </span>
        </h3>
        {predicciones.length > 0 && (
          <button
            onClick={handleClearClick}
            className="px-4 py-2 text-sm font-medium text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 border border-red-300 dark:border-red-600 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
          >
            {TEXTOS.botonLimpiar}
          </button>
        )}
      </div>

      {/* Confirmación de limpieza */}
      {showConfirm && (
        <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-red-700 dark:text-red-300 mb-3">
            {TEXTOS.confirmacionLimpiar}
          </p>
          <div className="flex gap-3">
            <button
              onClick={handleConfirmClear}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
            >
              Sí, limpiar todo
            </button>
            <button
              onClick={handleCancelClear}
              className="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-800 dark:text-white rounded-lg transition-colors"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      {/* Lista de predicciones */}
      <div className="space-y-4 max-h-96 overflow-y-auto">
        {predicciones.map((prediccion) => (
          <div
            key={prediccion.id}
            className="p-4 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
          >
            <div className="flex justify-between items-start mb-2">
              <div>
                <p className="font-semibold text-gray-900 dark:text-white">
                  {capitalizar(prediccion.equipos.local)} vs {capitalizar(prediccion.equipos.visitante)}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  {formatFecha(prediccion.fecha)}
                </p>
              </div>
              <span
                className={`text-sm font-semibold ${getRecomendacionColor(
                  prediccion.recomendacion
                )}`}
              >
                {prediccion.recomendacion}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 mt-2">
              <div className="text-center">
                <p className="text-xs text-gray-500 dark:text-gray-400">Local</p>
                <p className="text-sm font-semibold text-purple-600 dark:text-purple-400">
                  {formatearProbabilidad(prediccion.probabilidades.local)}
                </p>
              </div>
              <div className="text-center">
                <p className="text-xs text-gray-500 dark:text-gray-400">Empate</p>
                <p className="text-sm font-semibold text-yellow-600 dark:text-yellow-400">
                  {formatearProbabilidad(prediccion.probabilidades.empate)}
                </p>
              </div>
              <div className="text-center">
                <p className="text-xs text-gray-500 dark:text-gray-400">Visitante</p>
                <p className="text-sm font-semibold text-red-600 dark:text-red-400">
                  {formatearProbabilidad(prediccion.probabilidades.visitante)}
                </p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Historial;
