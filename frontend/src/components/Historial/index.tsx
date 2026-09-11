import React, { useState } from 'react';
import HistorialTable from './HistorialTable';
import HistorialModal from './HistorialModal';
import type { PrediccionHistorialItem } from '../../types';

/**
 * Pantalla de Historial: muestra la tabla de predicciones y gestiona
 * la apertura/cierre del modal con el detalle completo de cada una.
 */
const Historial: React.FC = () => {
  const [prediccionSeleccionada, setPrediccionSeleccionada] =
    useState<PrediccionHistorialItem | null>(null);

  const handleVerMas = (prediccion: PrediccionHistorialItem) => {
    setPrediccionSeleccionada(prediccion);
  };

  const handleCloseModal = () => {
    setPrediccionSeleccionada(null);
  };

  return (
    <section className="w-full max-w-7xl mx-auto">
      <HistorialTable onVerMas={handleVerMas} />
      <HistorialModal prediccion={prediccionSeleccionada} onClose={handleCloseModal} />
    </section>
  );
};

export default Historial;
