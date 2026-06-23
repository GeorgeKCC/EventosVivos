export enum TipoReporte {
  TotalEntradas = 1,
  TotalEntradasDisponibles = 2,
  PorcentajeDeOcupacion = 3,
  TotalDeIngresos = 4,
  EstadoDelEvento = 5
}

export interface RequestGenerateReporte {
  eventoId: number;
  tipoReporte: TipoReporte;
}
