using ModuloReservaContract.Dtos;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloReserva.Mappers
{
    internal static class ReservaMappers
    {
        public static Reserva RequestCrearReservaToReserva(this RequestCrearReserva requestCrearReserva)
        {
            return new Reserva
            {
                Cantidad = requestCrearReserva.Cantidad,
                NombreComprador = requestCrearReserva.NombreComprador,
                EmailComprador = requestCrearReserva.EmailComprado,
                EventoId = requestCrearReserva.EventoId,
                EstadoReservaId = (int)EstadoReservaEnum.PendientePago,
            };
        }
    }
}
