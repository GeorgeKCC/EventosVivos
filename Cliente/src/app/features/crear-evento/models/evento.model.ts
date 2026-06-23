export interface RequestCrearEvento {
  titulo: string;
  descripcion: string;
  capacidadMaxima: number;
  inicioEvento: string;
  iniciaHora: string;
  finEvento: string;
  finHora: string;
  precio: number;
  venueId: number;
  tipoEventoId: number;
}

export interface Venue {
  id: number;
  nombre: string;
  capacidad: number;
  ciudad: string;
}

export interface TipoEvento {
  id: number;
  nombre: string;
}
