export interface PartidoRequest {
  local: string;
  visitante: string;
}

export interface PrediccionResponse {
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
  explicacion: string;
  factores_clave: string[];
  alertas: string[];
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
}
