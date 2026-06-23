using Microsoft.EntityFrameworkCore;
using ModuloEventoContract;
using Transversal.Database;
using Transversal.Database.Entities;

namespace ModuloEvento.ImplementationUseCase
{
    internal class ObtenerTodoVenueUseCase(EventosVivosDbContext eventosVivosDbContext) : IObtenerTodoVenueUseCase
    {
        public async Task<IEnumerable<Venue>> ExecuteAsync()
        {
            return await eventosVivosDbContext.Venues.AsNoTracking().ToListAsync();
        }
    }
}
