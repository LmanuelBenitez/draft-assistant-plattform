import { useCallback, useEffect, useRef, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { predecirPartido, obtenerAnalisis } from '../services/api';
import type { PartidoRequest, PrediccionResponse } from '../types';

/** Intervalo entre intentos de polling (ms). */
const POLL_INTERVAL_MS = 3000;

/** Número máximo de intentos de polling (20 * 3s = 60s). */
const MAX_POLL_ATTEMPTS = 20;

interface UsePrediccionReturn {
  prediccion: PrediccionResponse | null;
  isLoading: boolean;
  /** Indica si el análisis de DeepSeek se está generando en segundo plano. */
  cargandoAnalisis: boolean;
  error: Error | null;
  realizarPrediccion: (request: PartidoRequest) => Promise<void>;
  resetPrediccion: () => void;
}

export const usePrediccion = (): UsePrediccionReturn => {
  const [prediccion, setPrediccion] = useState<PrediccionResponse | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [cargandoAnalisis, setCargandoAnalisis] = useState<boolean>(false);

  // Guarda el ID del intervalo de polling para poder limpiarlo.
  const intervalRef = useRef<number | null>(null);
  // Evita que el polling siga activo tras un reset o desmontaje.
  const canceladoRef = useRef<boolean>(false);

  /** Detiene cualquier polling activo y limpia el estado asociado. */
  const detenerPolling = useCallback(() => {
    if (intervalRef.current !== null) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
    setCargandoAnalisis(false);
  }, []);

  /**
   * Inicia el polling al endpoint de análisis para una predicción concreta.
   * Se detiene cuando `listo === true`, al agotar los intentos, o al cancelar.
   */
  const iniciarPolling = useCallback(
    (id: number) => {
      // Limpia cualquier intervalo previo antes de empezar uno nuevo.
      if (intervalRef.current !== null) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }

      canceladoRef.current = false;
      setCargandoAnalisis(true);

      let intentos = 0;

      const consultar = async () => {
        intentos += 1;
        try {
          const data = await obtenerAnalisis(id);

          // Si el componente se desmontó o se reseteó, no actualizamos nada.
          if (canceladoRef.current) return;

          if (data.listo && data.analisisDeepseek) {
            setPrediccion((prev) =>
              prev && prev.id === id
                ? { ...prev, analisisDeepSeek: data.analisisDeepseek }
                : prev
            );
            detenerPolling();
            return;
          }

          // Si ya está listo pero sin texto, o se agotaron los intentos, paramos.
          if (data.listo || intentos >= MAX_POLL_ATTEMPTS) {
            detenerPolling();
          }
        } catch (err) {
          // Un fallo puntual no debe romper el flujo: seguimos intentando
          // hasta agotar el número máximo de intentos.
          console.error('Error al obtener el análisis:', err);
          if (intentos >= MAX_POLL_ATTEMPTS) {
            detenerPolling();
          }
        }
      };

      // Primera consulta inmediata y luego cada POLL_INTERVAL_MS.
      void consultar();
      intervalRef.current = window.setInterval(() => {
        void consultar();
      }, POLL_INTERVAL_MS);
    },
    [detenerPolling]
  );

  const mutation = useMutation({
    mutationFn: predecirPartido,
    onSuccess: (data) => {
      setPrediccion(data);
      setError(null);

      // Si el análisis no viene en la respuesta inicial, iniciamos polling.
      if (!data.analisisDeepSeek && typeof data.id === 'number') {
        iniciarPolling(data.id);
      } else {
        setCargandoAnalisis(false);
      }
    },
    onError: (err) => {
      setError(err as Error);
      setPrediccion(null);
      detenerPolling();
    },
  });

  const realizarPrediccion = async (request: PartidoRequest): Promise<void> => {
    // Cancela cualquier polling anterior antes de una nueva predicción.
    canceladoRef.current = true;
    detenerPolling();
    try {
      await mutation.mutateAsync(request);
    } catch {
      // El error ya se maneja en el callback onError de la mutación.
    }
  };

  const resetPrediccion = (): void => {
    canceladoRef.current = true;
    detenerPolling();
    setPrediccion(null);
    setError(null);
    mutation.reset();
  };

  // Cleanup: detiene el polling si el componente se desmonta.
  useEffect(() => {
    return () => {
      canceladoRef.current = true;
      if (intervalRef.current !== null) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, []);

  return {
    prediccion,
    isLoading: mutation.isPending,
    cargandoAnalisis,
    error,
    realizarPrediccion,
    resetPrediccion,
  };
};
