export enum EstadoReservaEnum {
  PendientePago = 1,
  Confirmada = 2,
  Cancelada = 3
}

export interface RequestEstadoReserva {
  estadoReservaEnum: EstadoReservaEnum;
  reservaId: number;
}
