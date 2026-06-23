using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModuloEvento.Mappers;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using Transversal.Database;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;
using ValidationException = FluentValidation.ValidationException;

namespace ModuloEvento.ImplementationUseCase
{
    internal class CrearEventoUseCase(EventosVivosDbContext eventosVivosDbContext,
                                       IValidator<RequestCrearEvento> validator) : ICrearEventoUseCase
    {
        /// <summary>
        /// Crea un nuevo evento validando las reglas de negocio.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a crear.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(RequestCrearEvento requestCrearEvento)
        {
            await ValidateRequest(requestCrearEvento);
            await ValidateCapacity(requestCrearEvento);
            await ValidateOverlap(requestCrearEvento);
            ValidateNightHours(requestCrearEvento);

            eventosVivosDbContext.Eventos.Add(requestCrearEvento.RequestCrearEventoToEvento());
            await eventosVivosDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Valida los campos del request utilizando FluentValidation.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a validar.</param>
        /// <returns></returns>
        private async Task ValidateRequest(RequestCrearEvento requestCrearEvento)
        {
            var validate = await validator.ValidateAsync(requestCrearEvento);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }
        }

        /// <summary>
        /// Valida que los eventos en fines de semana no inicien después de las 22:00.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a validar.</param>
        /// <returns></returns>
        private static void ValidateNightHours(RequestCrearEvento requestCrearEvento)
        {
            var diaSemana = requestCrearEvento.InicioEvento.DayOfWeek;
            if (diaSemana is DayOfWeek.Saturday or DayOfWeek.Sunday
                && requestCrearEvento.IniciaHora >= new TimeOnly(22, 0))
            {
                throw new WeekendScheduleException("Eventos en fines de semana no pueden iniciar después de las 22:00");
            }
        }

        /// <summary>
        /// Valida que no existan eventos activos superpuestos en el mismo venue.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a validar.</param>
        /// <returns></returns>
        private async Task ValidateOverlap(RequestCrearEvento requestCrearEvento)
        {
            var inicioNuevo = requestCrearEvento.InicioEvento.ToDateTime(requestCrearEvento.IniciaHora);
            var finNuevo = requestCrearEvento.FinEvento.ToDateTime(requestCrearEvento.FinHora);

            var existeConflicto = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Where(e => e.VenueId == requestCrearEvento.VenueId
                         && e.EstadoId == (int)EstadoEnum.Activo)
                .AnyAsync(e =>
                    e.InicioEvento.ToDateTime(e.IniciaHora) < finNuevo
                    && e.FinEvento.ToDateTime(e.FinHora) > inicioNuevo);

            if (existeConflicto)
            {
                throw new VenueScheduleConflictException("Ya existe un evento activo en este venue con horarios superpuestos");
            }
        }

        /// <summary>
        /// Valida que la capacidad máxima del evento no exceda la capacidad del venue.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a validar.</param>
        /// <returns></returns>
        private async Task ValidateCapacity(RequestCrearEvento requestCrearEvento)
        {
            var venues = await eventosVivosDbContext.Venues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requestCrearEvento.VenueId)
                                     ?? throw new NotFoundCustomException($"No se encontro venue, id:{requestCrearEvento.VenueId}");

            if (requestCrearEvento.CapacidadMaxima > venues.Capacidad)
            {
                throw new VenueCapacityExceedsException("El evento creado supera la capidad del venue indicado");
            }
        }
    }

    internal class ValidationCrearEventoUseCase : AbstractValidator<RequestCrearEvento>
    {
        public ValidationCrearEventoUseCase()
        {
            RuleFor(x => x.Titulo).NotNull().NotEmpty().MinimumLength(5).MaximumLength(100);
            RuleFor(x => x.Descripcion).NotNull().NotEmpty().MinimumLength(10).MaximumLength(500);
            RuleFor(x => x.CapacidadMaxima).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.VenueId).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.InicioEvento).NotNull().NotEmpty();
            RuleFor(x => x.FinEvento).NotNull().NotEmpty();
            RuleFor(x => x.IniciaHora).NotNull().NotEmpty();
            RuleFor(x => x.FinHora).NotNull().NotEmpty();
            RuleFor(x => x.Precio).NotNull().NotEmpty().GreaterThan(0);

        }
    }
}