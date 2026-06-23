using Microsoft.EntityFrameworkCore;
using ModuloReserva.Strategies.StrategyContract;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReserva.Strategies.StrategyImplementation
{
    internal class StrategyReservaCancelada(EventosVivosDbContext eventosVivosDbContext) : IStrategyReserva
    {
        public int StrategyId => (int)EstadoReservaEnum.Cancelada;

        public async Task ExecuteAsync(int ReservaId)
        {
            Reserva reserva = await GetReservaById(ReservaId);

            ValidationStatusReservation(reserva);

            var inicioEvento = reserva.Evento.InicioEvento.ToDateTime(reserva.Evento.IniciaHora);
            var horasParaInicio = (inicioEvento - DateTime.Now).TotalHours;

            reserva.EstadoReservaId = (int)EstadoReservaEnum.Cancelada;
            reserva.FechaCancelacion = DateTime.Now;

            if (horasParaInicio < 48)
            {
                reserva.EsPerdida = true;
            }
            else
            {
                reserva.Cantidad = 0;
            }

            eventosVivosDbContext.Reservas.Update(reserva);
            await eventosVivosDbContext.SaveChangesAsync();
        }

        private static void ValidationStatusReservation(Reserva reserva)
        {
            if (reserva.EstadoReservaId == (int)EstadoReservaEnum.Cancelada
                            || reserva.EstadoReservaId == (int)EstadoReservaEnum.PendientePago)
            {
                throw new ReservaLimitException(
                    $"No se puede cancelar una reserva con estado '{(EstadoReservaEnum)reserva.EstadoReservaId}'");
            }
        }

        private async Task<Reserva> GetReservaById(int ReservaId)
        {
            return await eventosVivosDbContext.Reservas
                .Include(r => r.Evento)
                .FirstOrDefaultAsync(r => r.Id == ReservaId)
                ?? throw new NotFoundCustomException($"No se encontró reserva, id:{ReservaId}");
        }
    }
}
