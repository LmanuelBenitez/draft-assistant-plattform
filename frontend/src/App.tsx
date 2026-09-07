import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Formulario from './components/Formulario';
import Grafico from './components/Grafico';
import Tarjeta from './components/Tarjeta';
import Historial from './components/Historial';
import Layout from './components/Layout';
import { usePrediccion } from './hooks/usePrediccion';
import { useHistorialStore } from './store/historialStore';
import { TEXTOS } from './constants';
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
  const { prediccion, isLoading, error, realizarPrediccion, resetPrediccion } =
    usePrediccion();
  const { predicciones, addPrediccion, clearHistorial } = useHistorialStore();

  const handlePrediccion = async (data: { local: string; visitante: string }) => {
    try {
      // Store the team names for later use
      setEquiposActuales(data);
      await realizarPrediccion(data);
    } catch (err) {
      // Error handled by usePrediccion hook
    }
  };

  const [equiposActuales, setEquiposActuales] = React.useState<{ local: string; visitante: string } | null>(null);

  const handleClearHistorial = () => {
    clearHistorial();
  };

  const handleResetPrediccion = () => {
    resetPrediccion();
    setEquiposActuales(null);
  };

  // Cuando se recibe una nueva predicción, guardarla en el historial
  React.useEffect(() => {
    if (prediccion && equiposActuales) {
      // Verificar si ya está guardada para evitar duplicados
      const yaGuardada = predicciones.some((p) => {
        const mismoEquipo =
          p.equipos.local === equiposActuales.local &&
          p.equipos.visitante === equiposActuales.visitante;
        return mismoEquipo;
      });

      if (!yaGuardada) {
        addPrediccion({
          equipos: equiposActuales,
          probabilidades: prediccion.probabilidades,
          recomendacion: prediccion.recomendacion,
        });
      }
    }
  }, [prediccion, equiposActuales, addPrediccion, predicciones]);

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
            ❌ {TEXTOS.error}: {error.message}
          </p>
          <button
            onClick={handleResetPrediccion}
            className="mt-2 w-full py-2 bg-red-100 dark:bg-red-800/30 hover:bg-red-200 dark:hover:bg-red-800/50 text-red-700 dark:text-red-300 rounded-lg transition-colors"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Resultados de la predicción */}
      {prediccion && equiposActuales && (
        <div className="space-y-6 mb-8">
          <Grafico probabilidades={prediccion.probabilidades} tipo="dona" />
          <Tarjeta
            prediccion={prediccion}
            local={equiposActuales.local}
            visitante={equiposActuales.visitante}
          />
        </div>
      )}

      {/* Historial */}
      <Historial
        predicciones={predicciones}
        onClear={handleClearHistorial}
      />
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
