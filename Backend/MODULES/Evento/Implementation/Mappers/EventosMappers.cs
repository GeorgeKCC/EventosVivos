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
    }
}