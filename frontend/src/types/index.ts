export interface PartidoRequest {
  local: string;
  visitante: string;
  ligaId: string;
  temporada: string;
  competicion?: string;
  bajas?: string;
  contexto?: string;
  fechaHora?: string;
  estadio?: string;
  golesLocal?: number | null;
  golesVisitante?: number | null;
  estado?: string | null;
}

export interface PrediccionResponse {
  id: number;
  local: string;
  visitante: string;
  ligaIdLocal: number;
  ligaIdVisitante: number;
  temporada: string;
  competicion: string;
  estadio: string;
  bajas: string;
  contexto: string;
  golesLocalPredichos: number;
  golesVisitantePredichos: number;
  probabilidadLocal: number;
  probabilidadEmpate: number;
  probabilidadVisitante: number;
  confianza: number;
  promedioGolesLocal: number;
  promedioGolesVisitante: number;
  analisisDeepSeek: string;
  fechaPrediccion: string;
  esAcertada: boolean;
  puntosObtenidos: number | null;
  golesRealesLocal: number | null;
  golesRealesVisitante: number | null;
}

export interface PrediccionHistorial {
  id: string;
  fecha: Date;
  equipos: {
    local: string;
    visitante: string;
  };
  probabilidades: {
    local: number;
    empate: number;
    visitante: number;
  };
  recomendacion: string;
  golesLocalPredichos: number;
  golesVisitantePredichos: number;
  confianza: number;
  analisisDeepSeek: string;
}
