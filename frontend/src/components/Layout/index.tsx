import React from 'react';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        <header className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
            ⚽ Asistente de Apuestas de Fútbol
          </h1>
          <p className="text-lg text-gray-600 dark:text-gray-300">
            Predice resultados con inteligencia artificial
          </p>
        </header>
        {children}
      </div>
    </div>
  );
};

export default Layout;
