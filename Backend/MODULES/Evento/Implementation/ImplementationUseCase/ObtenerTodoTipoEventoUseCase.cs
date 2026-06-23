using Microsoft.EntityFrameworkCore;
using ModuloEventoContract;
using Transversal.Database;
using Transversal.Database.Entities;

namespace ModuloEvento.ImplementationUseCase
{
    internal class ObtenerTodoTipoEventoUseCase(EventosVivosDbContext eventosVivosDbContext) : IObtenerTodoTipoEventoUseCase
    {
        public async Task<IEnumerable<TipoEvento>> ExecuteAsync()
        {
            return await eventosVivosDbContext.TipoEventos.AsNoTracking().ToListAsync();
        }
    }
}
