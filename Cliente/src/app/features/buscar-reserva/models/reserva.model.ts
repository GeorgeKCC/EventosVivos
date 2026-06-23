export interface ResponseReserva {
  id: number;
  cantidad: number;
  nombreComprador: string;
  emailComprador: string;
  fechaCancelacion: string | null;
  esPerdida: boolean;
  eventoId: number;
  evento: Evento | null;
  estadoReservaId: number;
  estadoReserva: EstadoReserva | null;
}

export interface Evento {
  id: number;
  titulo: string;
}

export interface EstadoReserva {
  id: number;
  nombre: string;
}
