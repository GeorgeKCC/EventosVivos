using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModuloEvento.Mappers;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using Transversal.Database;
using Transversal.Exceptions.Customs;
using ValidationException = FluentValidation.ValidationException;

namespace ModuloEvento.ImplementationUseCase
{
    internal class CrearEventoUseCase(EventosVivosDbContext eventosVivosDbContext,
                                       IValidator<RequestCrearEvento> validator) : ICrearEventoUseCase
    {
        public async Task ExecuteAsync(RequestCrearEvento requestCrearEvento)
        {
            var validate = await validator.ValidateAsync(requestCrearEvento);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }

            var venues = await eventosVivosDbContext.Venues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requestCrearEvento.VenueId)
                         ?? throw new NotFoundCustomException($"No se encontro venue, id:{requestCrearEvento.VenueId}");

            if (requestCrearEvento.CapacidadMaxima > venues.Capacidad)
            {
                throw new VenueCapacityExceedsException("El evento creado supera la capidad del venue indicado");
            }

            eventosVivosDbContext.Eventos.Add(requestCrearEvento.RequestCrearEventoToEvento());
            await eventosVivosDbContext.SaveChangesAsync();
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