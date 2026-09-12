import { format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';

export const formatFecha = (fecha: Date | string): string => {
  const date = typeof fecha === 'string' ? parseISO(fecha) : fecha;
  return format(date, "d 'de' MMMM 'de' yyyy, HH:mm", { locale: es });
};

export const capitalizar = (texto: string): string => {
  if (!texto) return texto;
  return texto.charAt(0).toUpperCase() + texto.slice(1).toLowerCase();
};

export const toTitleCase = (str: string): string => {
  return str
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
};

export const formatearProbabilidad = (valor: number): string => {
  return `${Math.round(valor * 100)}%`;
};

export const obtenerColorProbabilidad = (valor: number): string => {
  if (valor >= 0.5) return 'text-green-600 dark:text-green-400';
  if (valor >= 0.3) return 'text-yellow-600 dark:text-yellow-400';
  return 'text-red-600 dark:text-red-400';
};

export const generarId = (): string => {
  return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
};
