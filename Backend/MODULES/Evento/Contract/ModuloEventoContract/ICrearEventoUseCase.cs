using ModuloEventoContract.Dtos;

namespace ModuloEventoContract
{
    public interface ICrearEventoUseCase
    {
        public Task ExecuteAsync(RequestCrearEvento requestCrearEvento);
    }
}
