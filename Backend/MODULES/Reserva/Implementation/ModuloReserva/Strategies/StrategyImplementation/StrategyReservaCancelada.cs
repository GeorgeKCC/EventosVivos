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

        /// <summary>
        /// Cancela una reserva aplicando reglas de pérdida según horas restantes al evento.
        /// </summary>
        /// <param name="ReservaId">Identificador de la reserva a cancelar.</param>
        /// <returns>Resultado de la ejecución de la estrategia.</returns>
        public async Task<string> ExecuteAsync(int ReservaId)
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
            return "Reserva cancelada exitosamente";
        }

        /// <summary>
        /// Valida que el estado actual de la reserva permita la cancelación.
        /// </summary>
        /// <param name="reserva">Entidad reserva a validar.</param>
        /// <returns></returns>
        private static void ValidationStatusReservation(Reserva reserva)
        {
            if (reserva.EstadoReservaId == (int)EstadoReservaEnum.Cancelada
                            || reserva.EstadoReservaId == (int)EstadoReservaEnum.PendientePago)
            {
                throw new ReservaLimitException(
                    $"No se puede cancelar una reserva con estado '{(EstadoReservaEnum)reserva.EstadoReservaId}'");
            }
        }

        /// <summary>
        /// Obtiene una reserva con su evento asociado por identificador.
        /// </summary>
        /// <param name="ReservaId">Identificador de la reserva.</param>
        /// <returns>Entidad Reserva con el Evento incluido.</returns>
        private async Task<Reserva> GetReservaById(int ReservaId)
        {
            return await eventosVivosDbContext.Reservas
                .Include(r => r.Evento)
                .FirstOrDefaultAsync(r => r.Id == ReservaId)
                ?? throw new NotFoundCustomException($"No se encontró reserva, id:{ReservaId}");
        }
    }
}
