import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { STORAGE_KEYS } from '../constants';
import type { PrediccionHistorial } from '../types';
import { generarId } from '../utils/formatters';

interface HistorialState {
  predicciones: PrediccionHistorial[];
  addPrediccion: (prediccion: Omit<PrediccionHistorial, 'id' | 'fecha'>) => void;
  clearHistorial: () => void;
  getPrediccionesRecientes: (limit?: number) => PrediccionHistorial[];
}

export const useHistorialStore = create<HistorialState>()(
  persist(
    (set, get) => ({
      predicciones: [],

      addPrediccion: (prediccion) => {
        const nuevaPrediccion: PrediccionHistorial = {
          ...prediccion,
          id: generarId(),
          fecha: new Date(),
        };
        set((state) => ({
          predicciones: [nuevaPrediccion, ...state.predicciones],
        }));
      },

      clearHistorial: () => {
        set({ predicciones: [] });
      },

      getPrediccionesRecientes: (limit = 10) => {
        const { predicciones } = get();
        return predicciones.slice(0, limit);
      },
    }),
    {
      name: STORAGE_KEYS.historial,
    }
  )
);
