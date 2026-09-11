import React from 'react';
import { format, parseISO } from 'date-fns';
import type { PrediccionHistorialItem } from '../../types';

interface HistorialTableRowProps {
  prediccion: PrediccionHistorialItem;
  onVerMas: (prediccion: PrediccionHistorialItem) => void;
}

/**
 * Formatea una fecha ISO a "dd/MM/yyyy HH:mm".
 * Devuelve un guion si la fecha es inválida.
 */
const formatearFecha = (fecha: string): string => {
  try {
    return format(parseISO(fecha), 'dd/MM/yyyy HH:mm');
  } catch {
    return '-';
  }
};

/**
 * Formatea un valor de probabilidad a porcentaje entero (ej: 45%).
 * Acepta valores en rango 0-1 o 0-100 y los normaliza.
 */
const formatearPorcentaje = (valor: number): string => {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return '-';
  const normalizado = valor <= 1 ? valor * 100 : valor;
  return `${Math.round(normalizado)}%`;
};

/** Normaliza cualquier probabilidad a un valor 0-1 para las barras de color. */
const normalizar = (valor: number): number => {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return 0;
  return valor <= 1 ? valor : valor / 100;
};

/** Color condicional según el valor de la probabilidad (0-1). */
const colorProbabilidad = (valor: number): string => {
  const v = normalizar(valor);
  if (v >= 0.5) return 'text-green-600 dark:text-green-400';
  if (v >= 0.3) return 'text-yellow-600 dark:text-yellow-400';
  return 'text-red-600 dark:text-red-400';
};

/** Determina la recomendación a partir de las probabilidades. */
const calcularRecomendacion = (prediccion: PrediccionHistorialItem): string => {
  const { probabilidadLocal, probabilidadEmpate, probabilidadVisitante } = prediccion;
  if (
    probabilidadLocal >= probabilidadEmpate &&
    probabilidadLocal >= probabilidadVisitante
  ) {
    return 'Local';
  }
  if (
    probabilidadEmpate >= probabilidadLocal &&
    probabilidadEmpate >= probabilidadVisitante
  ) {
    return 'Empate';
  }
  return 'Visitante';
};

/** Colores según la recomendación: morado local, amarillo empate, rojo visitante. */
const colorRecomendacion = (recomendacion: string): string => {
  switch (recomendacion) {
    case 'Local':
      return 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300';
    case 'Empate':
      return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300';
    case 'Visitante':
      return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300';
    default:
      return 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300';
  }
};

const HistorialTableRow: React.FC<HistorialTableRowProps> = ({ prediccion, onVerMas }) => {
  const recomendacion = calcularRecomendacion(prediccion);
  const confianzaNorm = normalizar(prediccion.confianza);

  return (
    <tr className="border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/40 transition-colors">
      {/* Fecha */}
      <td className="px-4 py-3 text-sm text-gray-600 dark:text-gray-300 whitespace-nowrap">
        {formatearFecha(prediccion.fechaPrediccion)}
      </td>

      {/* Partido */}
      <td className="px-4 py-3 text-sm font-medium text-gray-900 dark:text-white">
        <span className="block max-w-[180px] truncate" title={`${prediccion.local} vs ${prediccion.visitante}`}>
          {prediccion.local} <span className="text-gray-400">vs</span> {prediccion.visitante}
        </span>
      </td>

      {/* Marcador predicho */}
      <td className="px-4 py-3 text-sm text-center font-semibold text-gray-800 dark:text-gray-100 whitespace-nowrap">
        {prediccion.golesLocalPredichos} - {prediccion.golesVisitantePredichos}
      </td>

      {/* Probabilidades (oculto en móvil pequeño) */}
      <td className="hidden md:table-cell px-4 py-3 text-sm text-center whitespace-nowrap">
        <span className={colorProbabilidad(prediccion.probabilidadLocal)}>
          {formatearPorcentaje(prediccion.probabilidadLocal)}
        </span>
        <span className="text-gray-400 mx-1">/</span>
        <span className={colorProbabilidad(prediccion.probabilidadEmpate)}>
          {formatearPorcentaje(prediccion.probabilidadEmpate)}
        </span>
        <span className="text-gray-400 mx-1">/</span>
        <span className={colorProbabilidad(prediccion.probabilidadVisitante)}>
          {formatearPorcentaje(prediccion.probabilidadVisitante)}
        </span>
      </td>

      {/* Confianza con barra pequeña (oculto en móvil) */}
      <td className="hidden lg:table-cell px-4 py-3 text-sm">
        <div className="flex items-center gap-2">
          <span className="text-gray-700 dark:text-gray-200 font-medium w-10 text-right">
            {formatearPorcentaje(prediccion.confianza)}
          </span>
          <div className="w-16 h-1.5 bg-gray-200 dark:bg-gray-600 rounded-full overflow-hidden">
            <div
              className="h-full bg-blue-500 dark:bg-blue-400 rounded-full"
              style={{ width: `${Math.min(100, Math.max(0, confianzaNorm * 100))}%` }}
            />
          </div>
        </div>
      </td>

      {/* Recomendación */}
      <td className="px-4 py-3 text-sm">
        <span
          className={`inline-block px-2 py-1 rounded-full text-xs font-semibold ${colorRecomendacion(
            recomendacion
          )}`}
        >
          {recomendacion}
        </span>
      </td>

      {/* Acciones */}
      <td className="px-4 py-3 text-sm text-right">
        <button
          type="button"
          onClick={() => onVerMas(prediccion)}
          className="px-3 py-1.5 text-xs font-medium text-blue-600 dark:text-blue-400 border border-blue-300 dark:border-blue-600 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors"
        >
          Ver más
        </button>
      </td>
    </tr>
  );
};

export default HistorialTableRow;
