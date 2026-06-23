using ModuloEventoContract.Dtos;

namespace ModuloEventoContract
{
    public interface IBuscarEventoUseCase
    {
        public Task<IEnumerable<ResponseBuscarEvento>> ExecuteAsync(RequestBuscarEvento requestBuscarEvento);
    }
}
