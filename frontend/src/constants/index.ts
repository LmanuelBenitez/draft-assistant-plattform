
export interface Liga {
  id: string;
  nombre: string;
  pais: string;
}

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

export const LIGAS_PRINCIPALES: Liga[] = [
  { id: "1", nombre: "FIFA World Cup", pais: "Mundial" },
  { id: "2", nombre: "UEFA Champions League", pais: "Europa" },
  { id: "3", nombre: "UEFA Europa League", pais: "Europa" },
  { id: "5", nombre: "UEFA European Championship", pais: "Europa" },
  { id: "39", nombre: "Premier League", pais: "Inglaterra" },
  { id: "61", nombre: "Ligue 1", pais: "Francia" },
  { id: "71", nombre: "Brasileirão Série A", pais: "Brasil" },
  { id: "78", nombre: "Bundesliga", pais: "Alemania" },
  { id: "88", nombre: "Eredivisie", pais: "Países Bajos" },
  { id: "94", nombre: "Primeira Liga", pais: "Portugal" },
  { id: "128", nombre: "Major League Soccer (MLS)", pais: "Estados Unidos" },
  { id: "135", nombre: "Serie A", pais: "Italia" },
  { id: "140", nombre: "La Liga", pais: "España" },
  { id: "264", nombre: "Argentine Liga Profesional", pais: "Argentina" },
  { id: "262", nombre: "Liga MX", pais: "México" },
];

export const getLigaById = (id: string): Liga | undefined => {
  return LIGAS_PRINCIPALES.find(liga => liga.id === id);
};

export const getNombresLigas = (): string[] => {
  return LIGAS_PRINCIPALES.map(liga => liga.nombre);
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
  placeholderCompeticion: 'Competición',
  placeholderBajas: 'Bajas',
  placeholderContexto: 'Contexto adicional',
  placeholderFecha: 'Fecha y hora',
  placeholderEstadio: 'Estadio',
  validacionMinimo: 'Mínimo 2 caracteres',
  validacionRequerido: 'Campo requerido',
};

export const API_CONFIG = {
  baseURL: 'https://localhost:7277/api',
  timeout: 60000,
};

export const STORAGE_KEYS = {
  historial: 'predicciones_historial',
};

export const RECOMENDACION_COLORS = {
  local: 'text-purple-600 dark:text-purple-400',
  empate: 'text-yellow-600 dark:text-yellow-400',
  visitante: 'text-red-600 dark:text-red-400',
};
