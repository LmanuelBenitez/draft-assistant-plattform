import axios from 'axios';
import { API_CONFIG } from '../constants';
import type {
  PartidoRequest,
  PrediccionResponse,
  PrediccionHistorialItem,
  AnalisisResponse,
} from '../types';

const apiClient = axios.create({
  baseURL: API_CONFIG.baseURL,
  timeout: API_CONFIG.timeout,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export const predecirPartido = async (
  request: PartidoRequest
): Promise<PrediccionResponse> => {
  const response = await apiClient.post<PrediccionResponse>('/prediccion/generar', request);
  return response.data;
};

export const obtenerPredicciones = async (): Promise<PrediccionHistorialItem[]> => {
  const response = await apiClient.get<PrediccionHistorialItem[]>('/prediccion');
  return response.data;
};

/**
 * Obtiene el análisis de DeepSeek de una predicción concreta.
 * El backend lo genera en segundo plano, por lo que `listo` indica
 * si el análisis ya está disponible.
 */
export const obtenerAnalisis = async (id: number): Promise<AnalisisResponse> => {
  const response = await apiClient.get<AnalisisResponse>(`/prediccion/${id}/analisis`);
  return response.data;
};

export default apiClient;
