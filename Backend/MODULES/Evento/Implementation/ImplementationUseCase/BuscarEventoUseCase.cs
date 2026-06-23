using Microsoft.EntityFrameworkCore;
using ModuloEvento.Mappers;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using Transversal.Database;

namespace ModuloEvento.ImplementationUseCase
{
    internal class BuscarEventoUseCase(EventosVivosDbContext eventosVivosDbContext) : IBuscarEventoUseCase
    {
        public async Task<IEnumerable<ResponseBuscarEvento>> ExecuteAsync(RequestBuscarEvento requestBuscarEvento)
        {
            var query = eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.TipoEvento)
                .Include(e => e.EstadoEvento)
                .AsQueryable();

            if (requestBuscarEvento.TipoEventoId.HasValue)
                query = query.Where(e => e.TipoEventoId == requestBuscarEvento.TipoEventoId.Value);

            if (requestBuscarEvento.VenueId.HasValue)
                query = query.Where(e => e.VenueId == requestBuscarEvento.VenueId.Value);

            if (requestBuscarEvento.EstadoId.HasValue)
                query = query.Where(e => e.EstadoId == requestBuscarEvento.EstadoId.Value);

            if (!string.IsNullOrWhiteSpace(requestBuscarEvento.Titulo))
                query = query.Where(e => e.Titulo.Contains(requestBuscarEvento.Titulo));

            if (requestBuscarEvento.FechaInicioEventoRange.HasValue)
                query = query.Where(e => e.InicioEvento >= requestBuscarEvento.FechaInicioEventoRange.Value);

            if (requestBuscarEvento.FechaFinEventoRange.HasValue)
                query = query.Where(e => e.InicioEvento <= requestBuscarEvento.FechaFinEventoRange.Value);

            var eventos = await query.ToListAsync();

            return eventos.Select(e => e.EventoToResponseBuscarEvento());
        }
    }
}
