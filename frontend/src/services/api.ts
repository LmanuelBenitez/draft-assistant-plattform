import axios from 'axios';
import { API_CONFIG } from '../constants';
import type { PartidoRequest, PrediccionResponse, PrediccionHistorialItem } from '../types';

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

export default apiClient;
