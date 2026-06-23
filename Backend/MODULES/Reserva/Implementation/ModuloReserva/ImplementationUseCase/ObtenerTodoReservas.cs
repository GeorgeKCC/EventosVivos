using Microsoft.EntityFrameworkCore;
using ModuloReservaContract;
using Transversal.Database;
using Transversal.Database.Entities;

namespace ModuloReserva.ImplementationUseCase
{
    internal class ObtenerTodoReservas(EventosVivosDbContext eventosVivosDbContext) : IObtenerTodoReservas
    {
        public async Task<IEnumerable<Reserva>> ExecuteAsync()
        {
            return await eventosVivosDbContext.Reservas
                                              .Include(r => r.Evento)
                                              .Include(r => r.EstadoReserva).AsNoTracking().ToListAsync();
        }
    }
}
