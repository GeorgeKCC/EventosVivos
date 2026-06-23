using ModuloEventoContract.Dtos;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloEvento.Mappers
{
    internal static class EventosMappers
    {
        public static Evento RequestCrearEventoToEvento(this RequestCrearEvento requestCrearEvento) 
        {
            return new Evento
            {
                Titulo = requestCrearEvento.Titulo,
                Descripcion = requestCrearEvento.Descripcion,
                CapacidadMaxima = requestCrearEvento.CapacidadMaxima,
                InicioEvento = requestCrearEvento.InicioEvento,
                IniciaHora = requestCrearEvento.IniciaHora,
                FinEvento = requestCrearEvento.FinEvento,
                FinHora = requestCrearEvento.FinHora,
                Precio = requestCrearEvento.Precio,
                VenueId = requestCrearEvento.VenueId,
                TipoEventoId = requestCrearEvento.TipoEventoId,
                EstadoId = (int)EstadoEnum.Activo
            };
        }

        public static ResponseBuscarEvento EventoToResponseBuscarEvento(this Evento evento)
        {
            return new ResponseBuscarEvento(
                evento.Titulo,
                evento.Descripcion,
                evento.VenueId,
                evento.Venue.Nombre,
                evento.CapacidadMaxima,
                evento.InicioEvento,
                evento.IniciaHora,
                evento.FinEvento,
                evento.FinHora,
                evento.TipoEventoId,
                evento.TipoEvento.Nombre,
                evento.EstadoId,
                evento.EstadoEvento.Nombre);
        }
    }
}