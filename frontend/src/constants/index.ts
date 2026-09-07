export const COLORS = {
  primary: {
    purple: '#8B5CF6',
    yellow: '#FBBF24',
    red: '#EF4444',
  },
  dark: {
    purple: '#7C3AED',
    yellow: '#F59E0B',
    red: '#DC2626',
  },
};

export const TEXTOS = {
  titulo: 'Asistente de Apuestas de Fútbol',
  subtitulo: 'Predice resultados con inteligencia artificial',
  botonPrediccion: 'Predecir Partido',
  botonLimpiar: 'Limpiar Historial',
  confirmacionLimpiar: '¿Estás seguro de que deseas eliminar todo el historial?',
  cargando: 'Analizando partido...',
  error: 'Error al realizar la predicción',
  placeholderLocal: 'Equipo Local',
  placeholderVisitante: 'Equipo Visitante',
  validacionMinimo: 'Mínimo 2 caracteres',
  validacionRequerido: 'Campo requerido',
};

export const API_CONFIG = {
  baseURL: '/api',
  timeout: 30000,
};

export const STORAGE_KEYS = {
  historial: 'predicciones_historial',
};

export const RECOMENDACION_COLORS = {
  local: 'text-purple-600 dark:text-purple-400',
  empate: 'text-yellow-600 dark:text-yellow-400',
  visitante: 'text-red-600 dark:text-red-400',
};
