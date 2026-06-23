using Transversal.Database.Entities;

namespace ModuloEventoContract
{
    public interface IObtenerTodoVenueUseCase
    {
        Task<IEnumerable<Venue>> ExecuteAsync();
    }
}
