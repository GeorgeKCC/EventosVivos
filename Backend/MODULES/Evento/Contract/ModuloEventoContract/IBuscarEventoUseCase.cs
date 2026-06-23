using ModuloEventoContract.Dtos;

namespace ModuloEventoContract
{
    public interface IBuscarEventoUseCase
    {
        /// <summary>
        /// Busca eventos según los filtros proporcionados.
        /// </summary>
        /// <param name="requestBuscarEvento">Filtros de búsqueda para eventos.</param>
        /// <returns>Colección de eventos que coinciden con los filtros.</returns>
        public Task<IEnumerable<ResponseBuscarEvento>> ExecuteAsync(RequestBuscarEvento requestBuscarEvento);
    }
}
