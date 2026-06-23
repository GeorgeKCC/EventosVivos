using ModuloReserva.Strategies.StrategyContract;
using Transversal.Database;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReserva.Strategies.StrategyImplementation
{
    internal class StrategyReservaCancelada(EventosVivosDbContext eventosVivosDbContext) : IStrategyReserva
    {
        public int StrategyId => (int)EstadoReservaEnum.Cancelada;

        public async Task ExecuteAsync(int ReservaId)
        {
           var reserva = eventosVivosDbContext.Reservas.FirstOrDefault(r => r.Id == ReservaId) ?? throw new NotFoundCustomException($"No se encontró reserva, id:{ReservaId}");

            reserva.EstadoReservaId = (int)EstadoReservaEnum.Cancelada;

            eventosVivosDbContext.Reservas.Update(reserva);
            await eventosVivosDbContext.SaveChangesAsync();
        }
    }
}
