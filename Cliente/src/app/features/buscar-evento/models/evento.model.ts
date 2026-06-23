export interface RequestBuscarEvento {
  tipoEventoId?: number;
  fechaInicioEventoRange?: string;
  fechaFinEventoRange?: string;
  venueId?: number;
  estadoId?: number;
  titulo?: string;
}

export interface ResponseBuscarEvento {
  eventoId: number;
  titulo: string;
  descripción: string;
  venuedId: number;
  venueName: string;
  capacidadMaxima: number;
  fechaInicioEvento: string;
  horaInicioEvento: string;
  fechaFinEvento: string;
  horaFinEvento: string;
  tipoEventoId: number;
  tipoEventoNombre: string;
  estadoId: number;
  estadoNombre: string;
}
