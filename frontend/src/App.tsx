import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Formulario from './components/Formulario';
import Grafico from './components/Grafico';
import Tarjeta from './components/Tarjeta';
import Layout from './components/Layout';
import Historial from './components/Historial';
import { usePrediccion } from './hooks/usePrediccion';
import { TEXTOS } from './constants';
import type { PartidoRequest } from './types';
import './App.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function AppContent() {
  const { prediccion, isLoading, error, realizarPrediccion, resetPrediccion } = usePrediccion();
  const [equiposActuales, setEquiposActuales] = React.useState<PartidoRequest | null>(null);

  const handlePrediccion = async (data: PartidoRequest) => {
    setEquiposActuales(data);
    await realizarPrediccion(data);
  };

  const handleReset = () => {
    resetPrediccion();
    setEquiposActuales(null);
  };

  return (
    <Layout>
      {/* Formulario */}
      <div className="mb-6">
        <Formulario onSubmit={handlePrediccion} isLoading={isLoading} />
      </div>

      {/* Error */}
      {error && (
        <div className="max-w-2xl mx-auto mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-red-600 dark:text-red-400 text-center">
            {TEXTOS.error}: {error.message}
          </p>
          <button
            onClick={handleReset}
            className="mt-2 w-full py-2 bg-red-100 dark:bg-red-800/30 hover:bg-red-200 dark:hover:bg-red-800/50 text-red-700 dark:text-red-300 rounded-lg transition-colors"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Resultados de la predicción */}
      {prediccion && equiposActuales && (
        <div className="space-y-6 mb-8">
          <Grafico
            probabilidades={{
              local: prediccion.probabilidadLocal,
              empate: prediccion.probabilidadEmpate,
              visitante: prediccion.probabilidadVisitante,
            }}
            tipo="dona"
          />
          <Tarjeta
            prediccion={prediccion}
            local={equiposActuales.local}
            visitante={equiposActuales.visitante}
          />
        </div>
      )}

      {/* Historial de predicciones */}
      <div className="mt-10">
        <Historial />
      </div>
    </Layout>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppContent />
    </QueryClientProvider>
  );
}

export default App;