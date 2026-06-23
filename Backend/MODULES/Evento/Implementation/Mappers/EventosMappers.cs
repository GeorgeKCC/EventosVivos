using ModuloEventoContract.Dtos;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloEvento.Mappers
{
    internal static class EventosMappers
    {
        /// <summary>
        /// Convierte un RequestCrearEvento en una entidad Evento.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del request de creación.</param>
        /// <returns>Entidad Evento mapeada.</returns>
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

        /// <summary>
        /// Convierte una entidad Evento en un ResponseBuscarEvento.
        /// </summary>
        /// <param name="evento">Entidad Evento a convertir.</param>
        /// <returns>DTO de respuesta con los datos del evento.</returns>
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