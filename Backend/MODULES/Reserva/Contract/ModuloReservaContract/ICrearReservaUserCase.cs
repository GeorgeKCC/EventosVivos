using ModuloReservaContract.Dtos;

namespace ModuloReservaContract
{
    public interface ICrearReservaUserCase
    {
        public Task ExecuteAsync(RequestCrearReserva request);
    }
}
