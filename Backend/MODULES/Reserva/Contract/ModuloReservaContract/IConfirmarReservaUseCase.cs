using ModuloReservaContract.Dtos;

namespace ModuloReservaContract
{
    public interface IConfirmarReservaUseCase
    {
        /// <summary>
        /// Actualiza el estado de una reserva (confirmar o cancelar).
        /// </summary>
        /// <param name="requestEstadoReserva">Datos con el nuevo estado de la reserva.</param>
        /// <returns></returns>
        Task<string> ExecuteAsync(RequestEstadoReserva requestEstadoReserva);
    }
}
