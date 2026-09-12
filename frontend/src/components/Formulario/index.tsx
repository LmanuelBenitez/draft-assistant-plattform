import React from 'react';
import { useForm } from 'react-hook-form';
import { TEXTOS, LIGAS_PRINCIPALES } from '../../constants';
import { toTitleCase } from '../../utils/formatters';
import type { PartidoRequest } from '../../types';

interface FormularioProps {
  onSubmit: (data: PartidoRequest) => void;
  isLoading: boolean;
}

interface FormData {
  local: string;
  visitante: string;
  ligaId: string;
  temporada: string;
  competicion: string;
  bajas: string;
  contexto: string;
  fechaHora: string;
  estadio: string;
  golesLocal: string;
  golesVisitante: string;
  estado: string;  
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
      ligaId: '',
      temporada: new Date().getFullYear().toString(),
      competicion: '',
      bajas: '',
      contexto: '',
      fechaHora: '',
      estadio: '',
      golesLocal: '',
      golesVisitante: '',
      estado: '',
    },
  });

  const onSubmitHandler = (data: FormData) => {
    onSubmit({
      local: toTitleCase(data.local.trim()),
      visitante: toTitleCase(data.visitante.trim()),
      ligaId: data.ligaId.trim(),
      temporada: data.temporada.trim(),
      competicion: data.competicion.trim() || undefined,
      bajas: data.bajas.trim() || undefined,
      contexto: data.contexto.trim() || undefined,
      fechaHora: data.fechaHora.trim() || undefined,
      estadio: data.estadio.trim() || undefined,
      golesLocal: data.golesLocal ? Number(data.golesLocal) : null,
      golesVisitante: data.golesVisitante ? Number(data.golesVisitante) : null,
      estado: data.estado || null,
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

      <div className="space-y-2">
        <label htmlFor="ligaIdLocal" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Liga
        </label>
        <div className="relative">
          <select
            id="ligaIdLocal"
            {...register('ligaId', {
              required: 'La liga local es requerida',
            })}
            className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50 appearance-none pr-10"
            disabled={isLoading}
          >
            <option value="" className="text-gray-500 dark:text-gray-400">
              Selecciona una liga
            </option>
            {LIGAS_PRINCIPALES.map((liga) => (
              <option key={liga.id} value={liga.id} className="dark:bg-gray-700 dark:text-white">
                {liga.nombre} ({liga.pais})
              </option>
            ))}
          </select>
          {/* Flecha con emoji */}
          <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500 pointer-events-none">
            ▼
          </span>
        </div>
        {errors.ligaId && (
          <p className="text-sm text-red-600 dark:text-red-400">{errors.ligaId.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <label htmlFor="temporada" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Temporada
        </label>
        <input
          id="temporada"
          type="text"
          {...register('temporada', {
            required: TEXTOS.validacionRequerido,
          })}
          placeholder="Ej: 2024"
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
        {errors.temporada && (
          <p className="text-sm text-red-600 dark:text-red-400">{errors.temporada.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <label htmlFor="competicion" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Competición
        </label>
        <input
          id="competicion"
          type="text"
          {...register('competicion')}
          placeholder={TEXTOS.placeholderCompeticion}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="bajas" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Bajas
        </label>
        <input
          id="bajas"
          type="text"
          {...register('bajas')}
          placeholder={TEXTOS.placeholderBajas}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="fechaHora" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Fecha y hora
        </label>
        <input
          id="fechaHora"
          type="datetime-local"
          {...register('fechaHora')}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="estadio" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Estadio
        </label>
        <input
          id="estadio"
          type="text"
          {...register('estadio')}
          placeholder={TEXTOS.placeholderEstadio}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="contexto" className="block text-sm font-medium text-gray-700 dark:text-gray-200">
          Contexto
        </label>
        <input
          id="contexto"
          type="text"
          {...register('contexto')}
          placeholder={TEXTOS.placeholderContexto}
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-colors disabled:opacity-50"
          disabled={isLoading}
        />
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
