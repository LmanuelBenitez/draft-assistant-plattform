import React, { useState, useEffect, useCallback } from 'react';
import { obtenerPredicciones } from '../../services/api';
import type { PrediccionHistorialItem } from '../../types';
import HistorialTableRow from './HistorialTableRow';

interface HistorialTableProps {
  /** Callback al pulsar "Ver más" en una fila. */
  onVerMas: (prediccion: PrediccionHistorialItem) => void;
}

const HistorialTable: React.FC<HistorialTableProps> = ({ onVerMas }) => {
  const [predicciones, setPredicciones] = useState<PrediccionHistorialItem[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  /** Carga las predicciones desde el backend. */
  const cargarPredicciones = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await obtenerPredicciones();
      setPredicciones(Array.isArray(data) ? data : []);
    } catch (err) {
      const mensaje =
        err instanceof Error ? err.message : 'Error desconocido al cargar las predicciones';
      setError(mensaje);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void cargarPredicciones();
  }, [cargarPredicciones]);

  const isEmpty = !isLoading && !error && predicciones.length === 0;

  return (
    <div className="w-full mx-auto p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-xl shadow-lg">
      {/* Encabezado + botón actualizar */}
      <div className="flex justify-between items-center mb-4 gap-4">
        <h3 className="text-lg font-semibold text-gray-800 dark:text-white">
          Historial de Predicciones
          {!isLoading && !error && (
            <span className="ml-2 text-sm font-normal text-gray-500 dark:text-gray-400">
              ({predicciones.length})
            </span>
          )}
        </h3>
        <button
          type="button"
          onClick={() => void cargarPredicciones()}
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-blue-600 dark:text-blue-400 border border-blue-300 dark:border-blue-600 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Actualizando...' : 'Actualizar'}
        </button>
      </div>

      {/* Estado: cargando */}
      {isLoading && (
        <div className="flex flex-col items-center justify-center py-12">
          <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin" />
          <p className="mt-3 text-sm text-gray-500 dark:text-gray-400">Cargando predicciones...</p>
        </div>
      )}

      {/* Estado: error */}
      {!isLoading && error && (
        <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-red-600 dark:text-red-400 text-center">{error}</p>
          <button
            type="button"
            onClick={() => void cargarPredicciones()}
            className="mt-2 w-full py-2 bg-red-100 dark:bg-red-800/30 hover:bg-red-200 dark:hover:bg-red-800/50 text-red-700 dark:text-red-300 rounded-lg transition-colors"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Estado: vacío */}
      {isEmpty && (
        <p className="text-center text-gray-500 dark:text-gray-400 py-12">
          No hay predicciones
        </p>
      )}

      {/* Tabla */}
      {!isLoading && !error && predicciones.length > 0 && (
        <div className="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
          <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead className="bg-gray-50 dark:bg-gray-700/60">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Fecha</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Partido</th>
                <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Marcador</th>
                <th className="hidden md:table-cell px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Probabilidades</th>
                <th className="hidden lg:table-cell px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Confianza</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Recomendación</th>
                <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">Acciones</th>
              </tr>
            </thead>
            <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-100 dark:divide-gray-700">
              {predicciones.map((prediccion) => (
                <HistorialTableRow
                  key={prediccion.id}
                  prediccion={prediccion}
                  onVerMas={onVerMas}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default HistorialTable;
