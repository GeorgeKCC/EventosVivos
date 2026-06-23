using ModuloReservaContract.Dtos;

namespace ModuloReservaContract
{
    public interface ICrearReservaUserCase
    {
        /// <summary>
        /// Crea una nueva reserva para un evento.
        /// </summary>
        /// <param name="request">Datos de la reserva a crear.</param>
        /// <returns></returns>
        public Task ExecuteAsync(RequestCrearReserva request);
    }
}
