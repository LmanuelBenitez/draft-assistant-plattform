import React, { useEffect, useCallback } from 'react';
import { format, parseISO } from 'date-fns';
import type { PrediccionHistorialItem } from '../../types';

interface HistorialModalProps {
  prediccion: PrediccionHistorialItem | null;
  onClose: () => void;
}

/** Formatea una fecha ISO a "dd/MM/yyyy HH:mm". */
const formatearFecha = (fecha: string): string => {
  try {
    return format(parseISO(fecha), 'dd/MM/yyyy HH:mm');
  } catch {
    return '-';
  }
};

/** Normaliza probabilidades a rango 0-1. */
const normalizar = (valor: number): number => {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return 0;
  return valor <= 1 ? valor : valor / 100;
};

/** Formatea probabilidad a porcentaje entero. */
const formatearPorcentaje = (valor: number): string => {
  return `${Math.round(normalizar(valor) * 100)}%`;
};

/** Fila etiqueta/valor reutilizable. */
const Dato: React.FC<{ etiqueta: string; valor: React.ReactNode }> = ({ etiqueta, valor }) => (
  <div className="flex justify-between gap-4 py-1.5 border-b border-gray-100 dark:border-gray-700/60 last:border-0">
    <span className="text-sm text-gray-500 dark:text-gray-400">{etiqueta}</span>
    <span className="text-sm font-medium text-gray-900 dark:text-gray-100 text-right">{valor}</span>
  </div>
);

/** Barra de probabilidad con etiqueta. */
const BarraProbabilidad: React.FC<{ etiqueta: string; valor: number; color: string }> = ({
  etiqueta,
  valor,
  color,
}) => (
  <div>
    <div className="flex justify-between mb-1">
      <span className="text-xs font-medium text-gray-600 dark:text-gray-300">{etiqueta}</span>
      <span className="text-xs font-semibold text-gray-800 dark:text-gray-100">
        {formatearPorcentaje(valor)}
      </span>
    </div>
    <div className="h-2 w-full bg-gray-200 dark:bg-gray-600 rounded-full overflow-hidden">
      <div
        className={`h-full rounded-full ${color}`}
        style={{ width: `${Math.min(100, Math.max(0, normalizar(valor) * 100))}%` }}
      />
    </div>
  </div>
);

const HistorialModal: React.FC<HistorialModalProps> = ({ prediccion, onClose }) => {
  // Cerrar con la tecla Escape
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    },
    [onClose]
  );

  useEffect(() => {
    if (!prediccion) return;
    document.addEventListener('keydown', handleKeyDown);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
    };
  }, [prediccion, handleKeyDown]);

  if (!prediccion) return null;

  const recomendacion = (() => {
    const { probabilidadLocal, probabilidadEmpate, probabilidadVisitante } = prediccion;
    if (probabilidadLocal >= probabilidadEmpate && probabilidadLocal >= probabilidadVisitante) {
      return `Victoria de ${prediccion.local}`;
    }
    if (probabilidadEmpate >= probabilidadLocal && probabilidadEmpate >= probabilidadVisitante) {
      return 'Empate';
    }
    return `Victoria de ${prediccion.visitante}`;
  })();

  const tieneResultadoReal =
    prediccion.golesRealesLocal !== null && prediccion.golesRealesVisitante !== null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
      onClick={onClose}
      role="dialog"
      aria-modal="true"
    >
      <div
        className="relative w-full max-w-3xl max-h-[90vh] overflow-y-auto bg-white dark:bg-gray-800 rounded-2xl shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Encabezado */}
        <div className="sticky top-0 z-10 flex items-start justify-between gap-4 p-5 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-t-2xl">
          <div>
            <h2 className="text-xl font-bold text-gray-900 dark:text-white">
              {prediccion.local} <span className="text-gray-400">vs</span> {prediccion.visitante}
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              📅 {formatearFecha(prediccion.fechaPrediccion)}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Cerrar"
            className="flex-shrink-0 w-9 h-9 flex items-center justify-center rounded-full text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            ✕
          </button>
        </div>

        <div className="p-5 space-y-6">
          {/* Marcador predicho */}
          <div className="text-center bg-gray-50 dark:bg-gray-700/40 rounded-xl py-4">
            <p className="text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400 mb-1">
              Marcador predicho
            </p>
            <p className="text-4xl font-extrabold text-gray-900 dark:text-white">
              {prediccion.golesLocalPredichos} - {prediccion.golesVisitantePredichos}
            </p>
          </div>

          {/* Probabilidades con barras */}
          <section className="space-y-3">
            <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300">
              📊 Probabilidades
            </h3>
            <BarraProbabilidad
              etiqueta={`Local (${prediccion.local})`}
              valor={prediccion.probabilidadLocal}
              color="bg-purple-500 dark:bg-purple-400"
            />
            <BarraProbabilidad
              etiqueta="Empate"
              valor={prediccion.probabilidadEmpate}
              color="bg-yellow-500 dark:bg-yellow-400"
            />
            <BarraProbabilidad
              etiqueta={`Visitante (${prediccion.visitante})`}
              valor={prediccion.probabilidadVisitante}
              color="bg-red-500 dark:bg-red-400"
            />
          </section>

          {/* Confianza y promedios de goles */}
          <section className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div className="p-4 bg-blue-50 dark:bg-blue-900/20 rounded-xl text-center">
              <p className="text-xs text-gray-600 dark:text-gray-400">Confianza</p>
              <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                {formatearPorcentaje(prediccion.confianza)}
              </p>
            </div>
            <div className="p-4 bg-green-50 dark:bg-green-900/20 rounded-xl text-center">
              <p className="text-xs text-gray-600 dark:text-gray-400">Goles prom. local</p>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400">
                {prediccion.promedioGolesLocal.toFixed(1)}
              </p>
            </div>
            <div className="p-4 bg-green-50 dark:bg-green-900/20 rounded-xl text-center">
              <p className="text-xs text-gray-600 dark:text-gray-400">Goles prom. visitante</p>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400">
                {prediccion.promedioGolesVisitante.toFixed(1)}
              </p>
            </div>
          </section>

          {/* Recomendación */}
          <section className="p-4 bg-purple-50 dark:bg-purple-900/20 rounded-xl border border-purple-200 dark:border-purple-800">
            <p className="text-xs font-medium text-purple-700 dark:text-purple-300">Recomendación</p>
            <p className="text-lg font-bold text-purple-700 dark:text-purple-300">{recomendacion}</p>
          </section>

          {/* Detalles del partido */}
          <section>
            <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
              📋 Detalles del partido
            </h3>
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 px-4 py-2">
              <Dato etiqueta="Competición" valor={prediccion.competicion ?? '-'} />
              <Dato etiqueta="Temporada" valor={prediccion.temporada ?? '-'} />
              <Dato etiqueta="Estadio" valor={prediccion.estadio ?? '-'} />
              <Dato etiqueta="Liga ID" valor={prediccion.ligaId ?? '-'} />
              <Dato etiqueta="Bajas" valor={prediccion.bajas ?? 'Sin bajas reportadas'} />
            </div>
          </section>

          {/* Contexto */}
          {prediccion.contexto && (
            <section>
              <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
                📝 Contexto
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed whitespace-pre-line">
                {prediccion.contexto}
              </p>
            </section>
          )}

          {/* Análisis DeepSeek */}
          <section>
            <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
              🤖 Análisis DeepSeek
            </h3>
            <div className="p-4 bg-gray-50 dark:bg-gray-700/40 rounded-xl">
              <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed whitespace-pre-line">
                {prediccion.analisisDeepSeek ?? 'Sin análisis disponible.'}
              </p>
            </div>
          </section>

          {/* Resultado real (si existe) */}
          {tieneResultadoReal && (
            <section>
              <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
                ✅ Resultado real
              </h3>
              <div
                className={`rounded-xl border px-4 py-3 ${
                  prediccion.esAcertada
                    ? 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800'
                    : 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
                }`}
              >
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-300">Marcador final</span>
                  <span className="text-lg font-bold text-gray-900 dark:text-white">
                    {prediccion.golesRealesLocal} - {prediccion.golesRealesVisitante}
                  </span>
                </div>
                <div className="flex items-center justify-between mt-2">
                  <span className="text-sm text-gray-600 dark:text-gray-300">Resultado</span>
                  <span
                    className={`text-sm font-semibold ${
                      prediccion.esAcertada
                        ? 'text-green-600 dark:text-green-400'
                        : 'text-red-600 dark:text-red-400'
                    }`}
                  >
                    {prediccion.esAcertada ? '🎯 Acertada' : '❌ Fallida'}
                  </span>
                </div>
                {prediccion.puntosObtenidos !== null && (
                  <div className="flex items-center justify-between mt-2">
                    <span className="text-sm text-gray-600 dark:text-gray-300">Puntos obtenidos</span>
                    <span className="text-sm font-bold text-gray-900 dark:text-white">
                      {prediccion.puntosObtenidos}
                    </span>
                  </div>
                )}
              </div>
            </section>
          )}

          {/* Resultado pendiente */}
          {!tieneResultadoReal && (
            <p className="text-center text-sm text-gray-500 dark:text-gray-400">
              ⏳ Resultado pendiente de confirmación
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default HistorialModal;
