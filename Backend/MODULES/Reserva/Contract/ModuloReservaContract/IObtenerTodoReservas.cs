using Transversal.Database.Entities;

namespace ModuloReservaContract
{
    public interface IObtenerTodoReservas
    {
        Task<IEnumerable<Reserva>> ExecuteAsync();
    }
}
