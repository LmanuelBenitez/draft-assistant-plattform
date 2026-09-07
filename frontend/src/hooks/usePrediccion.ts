import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { predecirPartido } from '../services/api';
import type { PartidoRequest, PrediccionResponse } from '../types';

interface UsePrediccionReturn {
  prediccion: PrediccionResponse | null;
  isLoading: boolean;
  error: Error | null;
  realizarPrediccion: (request: PartidoRequest) => Promise<void>;
  resetPrediccion: () => void;
}

export const usePrediccion = (): UsePrediccionReturn => {
  const [prediccion, setPrediccion] = useState<PrediccionResponse | null>(null);
  const [error, setError] = useState<Error | null>(null);

  const mutation = useMutation({
    mutationFn: predecirPartido,
    onSuccess: (data) => {
      setPrediccion(data);
      setError(null);
    },
    onError: (err) => {
      setError(err as Error);
      setPrediccion(null);
    },
  });

  const realizarPrediccion = async (request: PartidoRequest) => {
    try {
      await mutation.mutateAsync(request);
    } catch (err) {
      // Error handled by onError callback
    }
  };

  const resetPrediccion = () => {
    setPrediccion(null);
    setError(null);
    mutation.reset();
  };

  return {
    prediccion,
    isLoading: mutation.isPending,
    error,
    realizarPrediccion,
    resetPrediccion,
  };
};
