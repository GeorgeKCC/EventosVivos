using ModuloReservaContract.Dtos;

namespace ModuloReservaContract
{
    public interface IConfirmarReservaUseCase
    {
        Task ExecuteAsync(RequestEstadoReserva requestEstadoReserva);
    }
}
