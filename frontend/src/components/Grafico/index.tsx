import React from 'react';
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';
import { COLORS } from '../../constants';
import { formatearProbabilidad } from '../../utils/formatters';

interface GraficoProps {
  probabilidades: {
    local: number;
    empate: number;
    visitante: number;
  };
  tipo?: 'dona' | 'barras';
}

interface DataItem {
  name: string;
  value: number;
  color: string;
}

const Grafico: React.FC<GraficoProps> = ({ probabilidades, tipo = 'dona' }) => {
  const { local, empate, visitante } = probabilidades;

  const data: DataItem[] = [
    { name: 'Local', value: local, color: COLORS.primary.purple },
    { name: 'Empate', value: empate, color: COLORS.primary.yellow },
    { name: 'Visitante', value: visitante, color: COLORS.primary.red },
  ];

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-white dark:bg-gray-800 p-3 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700">
          <p className="font-semibold text-gray-900 dark:text-white">{payload[0].name}</p>
          <p className="text-gray-600 dark:text-gray-300">
            Probabilidad: <span className="font-bold">{formatearProbabilidad(payload[0].value)}</span>
          </p>
        </div>
      );
    }
    return null;
  };

  const renderDonaChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <PieChart>
        <Pie
          data={data}
          cx="50%"
          cy="50%"
          innerRadius={60}
          outerRadius={100}
          paddingAngle={5}
          dataKey="value"
          label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
          labelLine={true}
        >
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={entry.color} />
          ))}
        </Pie>
        <Tooltip content={<CustomTooltip />} />
        <Legend />
      </PieChart>
    </ResponsiveContainer>
  );

  const renderBarChart = () => (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={data} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" className="stroke-gray-300 dark:stroke-gray-600" />
        <XAxis dataKey="name" className="text-gray-600 dark:text-gray-300" />
        <YAxis
          tickFormatter={(value) => `${Math.round(value * 100)}%`}
          domain={[0, 1]}
          className="text-gray-600 dark:text-gray-300"
        />
        <Tooltip content={<CustomTooltip />} />
        <Bar dataKey="value" radius={[8, 8, 0, 0]}>
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={entry.color} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );

  return (
    <div className="w-full max-w-2xl mx-auto p-4 bg-white dark:bg-gray-800 rounded-xl shadow-lg">
      <h3 className="text-lg font-semibold text-center text-gray-800 dark:text-white mb-4">
        Probabilidades del Partido
      </h3>
      {tipo === 'dona' ? renderDonaChart() : renderBarChart()}
    </div>
  );
};

export default Grafico;
