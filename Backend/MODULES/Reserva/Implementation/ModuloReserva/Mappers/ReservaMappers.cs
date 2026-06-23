using ModuloReservaContract.Dtos;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloReserva.Mappers
{
    internal static class ReservaMappers
    {
        /// <summary>
        /// Convierte un RequestCrearReserva en una entidad Reserva.
        /// </summary>
        /// <param name="requestCrearReserva">Datos del request de creación de reserva.</param>
        /// <returns>Entidad Reserva mapeada.</returns>
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
