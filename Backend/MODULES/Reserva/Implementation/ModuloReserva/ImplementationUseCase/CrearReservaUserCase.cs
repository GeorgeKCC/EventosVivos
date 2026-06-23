using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModuloReserva.Mappers;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Exceptions.Customs;
using ValidationException = FluentValidation.ValidationException;

namespace ModuloReserva.ImplementationUseCase
{
    internal class CrearReservaUserCase(EventosVivosDbContext eventosVivosDbContext,
                                       IValidator<RequestCrearReserva> validator) : ICrearReservaUserCase
    {
        /// <summary>
        /// Crea una nueva reserva validando disponibilidad y reglas de negocio.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva a crear.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(RequestCrearReserva requestCrearReserva)
        {
            await ValidateRequest(requestCrearReserva);

            Evento evento = await GetEventoById(requestCrearReserva);

            await ValidateTickets(requestCrearReserva, evento);

            var inicioEvento = evento.InicioEvento.ToDateTime(evento.IniciaHora);
            var horasParaInicio = (inicioEvento - DateTime.Now).TotalHours;

            ValidateHoursAndReserve(requestCrearReserva, horasParaInicio);
            ValidateInitHour(horasParaInicio);
            ValidatePriceAndMaximumTicket(requestCrearReserva, evento);

            var reserva = requestCrearReserva.RequestCrearReservaToReserva();

            eventosVivosDbContext.Reservas.Add(reserva);
            await eventosVivosDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Valida que eventos con precio mayor a $100 no permitan más de 10 entradas por transacción.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva.</param>
        /// <param name="evento">Evento asociado a la reserva.</param>
        /// <returns></returns>
        private static void ValidatePriceAndMaximumTicket(RequestCrearReserva requestCrearReserva, Evento evento)
        {
            if (evento.Precio > 100 && requestCrearReserva.Cantidad > 10)
            {
                throw new ReservaLimitException(
                    "Eventos con precio mayor a $100 permiten máximo 10 entradas por transacción");
            }
        }

        /// <summary>
        /// Valida que no se permitan reservas para eventos que inicien en menos de 1 hora.
        /// </summary>
        /// <param name="horasParaInicio">Horas restantes para el inicio del evento.</param>
        /// <returns></returns>
        private static void ValidateInitHour(double horasParaInicio)
        {
            if (horasParaInicio < 1)
            {
                throw new ReservaLimitException(
                    "No se permiten reservas para eventos que inicien en menos de 1 hora");
            }
        }

        /// <summary>
        /// Valida que si el evento inicia en menos de 24 horas, solo se permitan máximo 5 entradas.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva.</param>
        /// <param name="horasParaInicio">Horas restantes para el inicio del evento.</param>
        /// <returns></returns>
        private static void ValidateHoursAndReserve(RequestCrearReserva requestCrearReserva, double horasParaInicio)
        {
            if (horasParaInicio < 24 && requestCrearReserva.Cantidad > 5)
            {
                throw new ReservaLimitException(
                    "El evento inicia en menos de 24 horas, solo se permite reservar máximo 5 entradas por transacción");
            }
        }

        /// <summary>
        /// Valida que haya suficientes entradas disponibles para la cantidad solicitada.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva.</param>
        /// <param name="evento">Evento asociado a la reserva.</param>
        /// <returns></returns>
        private async Task ValidateTickets(RequestCrearReserva requestCrearReserva, Evento evento)
        {
            var totalReservadas = await eventosVivosDbContext.Reservas
                            .Where(r => r.EventoId == requestCrearReserva.EventoId)
                            .SumAsync(r => r.Cantidad);

            var entradasDisponibles = evento.CapacidadMaxima - totalReservadas;

            if (requestCrearReserva.Cantidad > entradasDisponibles)
            {
                throw new EventoSoldOutException(
                    $"No hay suficientes entradas disponibles. Disponibles: {entradasDisponibles}, solicitadas: {requestCrearReserva.Cantidad}");
            }
        }

        /// <summary>
        /// Obtiene un evento por su identificador.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva que contiene el EventoId.</param>
        /// <returns>Entidad Evento encontrada.</returns>
        private async Task<Evento> GetEventoById(RequestCrearReserva requestCrearReserva)
        {
            return await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == requestCrearReserva.EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{requestCrearReserva.EventoId}");
        }

        /// <summary>
        /// Valida los campos del request utilizando FluentValidation.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva a validar.</param>
        /// <returns></returns>
        private async Task ValidateRequest(RequestCrearReserva requestCrearReserva)
        {
            var validate = await validator.ValidateAsync(requestCrearReserva);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }
        }
    }

    internal class ValidationCrearReservaUserCase : AbstractValidator<RequestCrearReserva>
    {
        public ValidationCrearReservaUserCase()
        {
            RuleFor(x => x.EventoId).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.Cantidad).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.NombreComprador).NotNull().NotEmpty().MinimumLength(3).MaximumLength(100);
            RuleFor(x => x.EmailComprado).NotNull().NotEmpty().EmailAddress();
        }
    }
}
