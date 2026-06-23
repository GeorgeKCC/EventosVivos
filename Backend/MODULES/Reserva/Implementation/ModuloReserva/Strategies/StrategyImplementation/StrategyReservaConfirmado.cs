using ModuloReserva.Strategies.StrategyContract;
using Transversal.Database;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReserva.Strategies.StrategyImplementation
{
    internal class StrategyReservaConfirmado(EventosVivosDbContext eventosVivosDbContext) : IStrategyReserva
    {
        public int StrategyId => (int)EstadoReservaEnum.Confirmada;

        public async Task ExecuteAsync(int ReservaId)
        {
            var reserva = eventosVivosDbContext.Reservas.FirstOrDefault(r => r.Id == ReservaId) ?? throw new NotFoundCustomException($"No se encontró reserva, id:{ReservaId}");

            if (reserva.EstadoReservaId == (int)EstadoReservaEnum.Confirmada)
            {
                throw new ReservationConfirmedException($"La reserva con id:{ReservaId} ya está confirmada.");
            }

            if (reserva.EstadoReservaId == (int)EstadoReservaEnum.Cancelada)
            {
                throw new ReservationCancelException($"La reserva con id:{ReservaId} ya está cancelada.");
            }

            reserva.EstadoReservaId = (int)EstadoReservaEnum.Confirmada;

            eventosVivosDbContext.Reservas.Update(reserva);
            await eventosVivosDbContext.SaveChangesAsync();
        }
    }
}
