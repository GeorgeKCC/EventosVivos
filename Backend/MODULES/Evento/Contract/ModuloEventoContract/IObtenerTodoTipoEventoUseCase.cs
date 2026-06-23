using Transversal.Database.Entities;

namespace ModuloEventoContract
{
    public interface IObtenerTodoTipoEventoUseCase
    {
        public Task<IEnumerable<TipoEvento>> ExecuteAsync();
    }
}
