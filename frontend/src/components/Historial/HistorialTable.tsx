import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { obtenerPredicciones } from '../../services/api';
import type { PrediccionHistorialItem } from '../../types';
import HistorialTableRow from './HistorialTableRow';

interface HistorialTableProps {
  /** Callback al pulsar "Ver más" en una fila. */
  onVerMas: (prediccion: PrediccionHistorialItem) => void;
}

/** Opciones de items por página. */
const ITEMS_POR_PAGINA = 10;

const HistorialTable: React.FC<HistorialTableProps> = ({ onVerMas }) => {
  const [predicciones, setPredicciones] = useState<PrediccionHistorialItem[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [paginaActual, setPaginaActual] = useState<number>(1);

  /** Carga las predicciones desde el backend. */
  const cargarPredicciones = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await obtenerPredicciones();
      setPredicciones(Array.isArray(data) ? data : []);
      setPaginaActual(1); // ✅ Reiniciar a la primera página al recargar
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

  // ✅ Calcular total de páginas
  const totalPaginas = useMemo(
    () => Math.max(1, Math.ceil(predicciones.length / ITEMS_POR_PAGINA)),
    [predicciones.length]
  );

  // ✅ Obtener solo las predicciones de la página actual
  const prediccionesPaginadas = useMemo(() => {
    const inicio = (paginaActual - 1) * ITEMS_POR_PAGINA;
    return predicciones.slice(inicio, inicio + ITEMS_POR_PAGINA);
  }, [predicciones, paginaActual]);

  // ✅ Rango de items mostrados (para "Mostrando X-Y de Z")
  const rangoInicio = predicciones.length === 0 ? 0 : (paginaActual - 1) * ITEMS_POR_PAGINA + 1;
  const rangoFin = Math.min(paginaActual * ITEMS_POR_PAGINA, predicciones.length);

  const irPaginaAnterior = () => {
    setPaginaActual((prev) => Math.max(1, prev - 1));
  };

  const irPaginaSiguiente = () => {
    setPaginaActual((prev) => Math.min(totalPaginas, prev + 1));
  };

  const irAPagina = (pagina: number) => {
    setPaginaActual(pagina);
  };

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
        <p className="text-center text-gray-500 dark:text-gray-400 py-12">No hay predicciones</p>
      )}

      {/* Tabla */}
      {!isLoading && !error && predicciones.length > 0 && (
        <>
          <div className="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-700/60">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Fecha
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Partido
                  </th>
                  <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Marcador
                  </th>
                  <th className="hidden md:table-cell px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Probabilidades
                  </th>
                  <th className="hidden lg:table-cell px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Confianza
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Recomendación
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-100 dark:divide-gray-700">
                {prediccionesPaginadas.map((prediccion) => (
                  <HistorialTableRow
                    key={prediccion.id}
                    prediccion={prediccion}
                    onVerMas={onVerMas}
                  />
                ))}
              </tbody>
            </table>
          </div>

          {/* ✅ Paginación */}
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mt-4">
            {/* Info de items */}
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Mostrando{' '}
              <span className="font-medium text-gray-900 dark:text-white">
                {rangoInicio}
              </span>{' '}
              -{' '}
              <span className="font-medium text-gray-900 dark:text-white">
                {rangoFin}
              </span>{' '}
              de{' '}
              <span className="font-medium text-gray-900 dark:text-white">
                {predicciones.length}
              </span>{' '}
              predicciones
            </p>

            {/* Controles de paginación */}
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={irPaginaAnterior}
                disabled={paginaActual === 1}
                className="px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ← Anterior
              </button>

              {/* Números de página */}
              <div className="flex items-center gap-1">
                {Array.from({ length: totalPaginas }, (_, i) => i + 1)
                  .filter((pagina) => {
                    // Mostrar solo páginas cercanas a la actual
                    if (totalPaginas <= 7) return true;
                    if (pagina === 1 || pagina === totalPaginas) return true;
                    if (Math.abs(pagina - paginaActual) <= 1) return true;
                    return false;
                  })
                  .map((pagina, index, array) => {
                    // Insertar "..." donde haya saltos
                    const anterior = array[index - 1];
                    const mostrarEllipsis = anterior && pagina - anterior > 1;

                    return (
                      <React.Fragment key={pagina}>
                        {mostrarEllipsis && (
                          <span className="px-2 text-gray-400 dark:text-gray-500">...</span>
                        )}
                        <button
                          type="button"
                          onClick={() => irAPagina(pagina)}
                          className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                            pagina === paginaActual
                              ? 'bg-blue-600 text-white'
                              : 'text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700'
                          }`}
                        >
                          {pagina}
                        </button>
                      </React.Fragment>
                    );
                  })}
              </div>

              <button
                type="button"
                onClick={irPaginaSiguiente}
                disabled={paginaActual === totalPaginas}
                className="px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Siguiente →
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default HistorialTable;