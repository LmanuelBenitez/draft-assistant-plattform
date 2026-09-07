import React from 'react';

interface LoaderProps {
  mensaje?: string;
  tamanio?: 'small' | 'medium' | 'large';
}

const Loader: React.FC<LoaderProps> = ({ mensaje = 'Cargando...', tamanio = 'medium' }) => {
  const tamanioMap = {
    small: 'w-6 h-6',
    medium: 'w-12 h-12',
    large: 'w-16 h-16',
  };

  return (
    <div className="flex flex-col items-center justify-center p-4">
      <div className={`${tamanioMap[tamanio]} animate-spin rounded-full border-4 border-purple-200 border-t-purple-600 dark:border-purple-700 dark:border-t-purple-400`}></div>
      {mensaje && (
        <p className="mt-3 text-sm text-gray-600 dark:text-gray-300">{mensaje}</p>
      )}
    </div>
  );
};

export default Loader;
