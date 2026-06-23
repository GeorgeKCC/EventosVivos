using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract.Dtos;
using Moq;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloEventoTest
{
    public class CrearEventoUseCaseTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly Mock<IValidator<RequestCrearEvento>> _validatorMock;
        private readonly CrearEventoUseCase _useCase;

        public CrearEventoUseCaseTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _validatorMock = new Mock<IValidator<RequestCrearEvento>>();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RequestCrearEvento>(), default))
                .ReturnsAsync(new ValidationResult());

            _useCase = new CrearEventoUseCase(_context, _validatorMock.Object);

            _context.Venues.Add(new Venue
            {
                Id = 1,
                Nombre = "Arena CDMX",
                Capacidad = 200,
                Ciudad = "Ciudad de México"
            });
            _context.SaveChanges();
        }

        private static RequestCrearEvento CrearRequestValido() => new(
            Titulo: "Concierto de Rock",
            Descripcion: "Gran concierto de rock en vivo con bandas nacionales",
            CapacidadMaxima: 100,
            InicioEvento: new DateOnly(2025, 8, 13),
            IniciaHora: new TimeOnly(20, 0),
            FinEvento: new DateOnly(2025, 8, 13),
            FinHora: new TimeOnly(23, 0),
            Precio: 150m,
            VenueId: 1,
            TipoEventoId: 1
        );

        [Fact]
        public async Task ExecuteAsync_RequestValido_DebeCrearEvento()
        {
            var request = CrearRequestValido();

            await _useCase.ExecuteAsync(request);

            var evento = await _context.Eventos.FirstOrDefaultAsync();
            evento.Should().NotBeNull();
            evento!.Titulo.Should().Be(request.Titulo);
            evento.EstadoId.Should().Be((int)EstadoEnum.Activo);
        }

        [Fact]
        public async Task ExecuteAsync_ValidacionFalla_DebeLanzarValidationException()
        {
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RequestCrearEvento>(), default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Titulo", "El titulo es requerido")
                }));

            var request = CrearRequestValido();

            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ExecuteAsync_VenueNoExiste_DebeLanzarNotFoundCustomException()
        {
            var request = CrearRequestValido() with { VenueId = 999 };

            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_CapacidadExcedeVenue_DebeLanzarVenueCapacityExceedsException()
        {
            var request = CrearRequestValido() with { CapacidadMaxima = 500 };

            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<VenueCapacityExceedsException>();
        }

        [Fact]
        public async Task ExecuteAsync_HorarioSuperpuesto_DebeLanzarVenueScheduleConflictException()
        {
            _context.Eventos.Add(new Evento
            {
                Titulo = "Evento Existente",
                Descripcion = "Descripcion del evento existente",
                CapacidadMaxima = 50,
                InicioEvento = new DateOnly(2025, 8, 13),
                IniciaHora = new TimeOnly(19, 0),
                FinEvento = new DateOnly(2025, 8, 13),
                FinHora = new TimeOnly(22, 0),
                Precio = 100m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });
            await _context.SaveChangesAsync();

            var request = CrearRequestValido();

            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<VenueScheduleConflictException>();
        }

        [Fact]
        public async Task ExecuteAsync_SinSuperposicionHorario_DebeCrearEvento()
        {
            _context.Eventos.Add(new Evento
            {
                Titulo = "Evento Temprano",
                Descripcion = "Descripcion del evento temprano",
                CapacidadMaxima = 50,
                InicioEvento = new DateOnly(2025, 8, 13),
                IniciaHora = new TimeOnly(10, 0),
                FinEvento = new DateOnly(2025, 8, 13),
                FinHora = new TimeOnly(12, 0),
                Precio = 100m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });
            await _context.SaveChangesAsync();

            var request = CrearRequestValido();

            await _useCase.ExecuteAsync(request);

            var count = await _context.Eventos.CountAsync();
            count.Should().Be(2);
        }

        [Theory]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public async Task ExecuteAsync_WeekendDespuesDeLas22_DebeLanzarWeekendScheduleException(DayOfWeek dia)
        {
            var fecha = ObtenerProximaFechaPorDia(dia);
            var request = CrearRequestValido() with
            {
                InicioEvento = fecha,
                IniciaHora = new TimeOnly(22, 30),
                FinEvento = fecha,
                FinHora = new TimeOnly(23, 30)
            };

            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<WeekendScheduleException>();
        }

        [Theory]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public async Task ExecuteAsync_WeekendAntesDeLas22_DebeCrearEvento(DayOfWeek dia)
        {
            var fecha = ObtenerProximaFechaPorDia(dia);
            var request = CrearRequestValido() with
            {
                Titulo = $"Evento Weekend {dia}",
                InicioEvento = fecha,
                IniciaHora = new TimeOnly(21, 0),
                FinEvento = fecha,
                FinHora = new TimeOnly(23, 0)
            };

            await _useCase.ExecuteAsync(request);

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Titulo == $"Evento Weekend {dia}");
            evento.Should().NotBeNull();
        }

        [Fact]
        public async Task ExecuteAsync_DiaEntreSemanaALas22_DebeCrearEvento()
        {
            var fecha = ObtenerProximaFechaPorDia(DayOfWeek.Wednesday);
            var request = CrearRequestValido() with
            {
                Titulo = "Evento Miercoles Noche",
                InicioEvento = fecha,
                IniciaHora = new TimeOnly(22, 30),
                FinEvento = fecha,
                FinHora = new TimeOnly(23, 30)
            };

            await _useCase.ExecuteAsync(request);

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Titulo == "Evento Miercoles Noche");
            evento.Should().NotBeNull();
        }

        private static DateOnly ObtenerProximaFechaPorDia(DayOfWeek dia)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var diasHasta = ((int)dia - (int)hoy.DayOfWeek + 7) % 7;
            if (diasHasta == 0) diasHasta = 7;
            return hoy.AddDays(diasHasta);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
