import React from 'react';
import { useForm } from 'react-hook-form';
import { TEXTOS } from '../../constants';
import { capitalizar } from '../../utils/formatters';

interface FormularioProps {
  onSubmit: (data: { local: string; visitante: string }) => void;
  isLoading: boolean;
}

interface FormData {
  local: string;
  visitante: string;
}

const Formulario: React.FC<FormularioProps> = ({ onSubmit, isLoading }) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<FormData>({
    defaultValues: {
      local: '',
      visitante: '',
    },
  });

  const onSubmitHandler = (data: FormData) => {
    onSubmit({
      local: capitalizar(data.local.trim()),
      visitante: capitalizar(data.visitante.trim()),
    });
    reset();
  };

  return (
    <form
      onSubmit={handleSubmit(onSubmitHandler)}
      className="w-full max-w-2xl mx-auto p-6 bg-white dark:bg-gray-800 rounded-xl shadow-lg space-y-6"
    >
      <div className="space-y-2">
        <label htmlFor="local" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Equipo Local
        </label>
        <input
          id="local"
          type="text"
          {...register('local', {
            required: TEXTOS.validacionRequerido,
            minLength: {
              value: 2,
              message: TEXTOS.validacionMinimo,
            },
          })}
          placeholder={TEXTOS.placeholderLocal}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
        {errors.local && (
          <p className="text-sm text-red-600 dark:text-red-400">{errors.local.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <label htmlFor="visitante" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Equipo Visitante
        </label>
        <input
          id="visitante"
          type="text"
          {...register('visitante', {
            required: TEXTOS.validacionRequerido,
            minLength: {
              value: 2,
              message: TEXTOS.validacionMinimo,
            },
          })}
          placeholder={TEXTOS.placeholderVisitante}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
        {errors.visitante && (
          <p className="text-sm text-red-600 dark:text-red-400">{errors.visitante.message}</p>
        )}
      </div>

      <button
        type="submit"
        disabled={isLoading}
        className="w-full py-3 px-6 bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800 text-white font-semibold rounded-lg shadow-md transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
      >
        {isLoading ? (
          <div className="flex items-center gap-2">
            <div className="w-5 h-5 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
            <span>{TEXTOS.cargando}</span>
          </div>
        ) : (
          TEXTOS.botonPrediccion
        )}
      </button>
    </form>
  );
};

export default Formulario;
