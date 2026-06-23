using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ModuloReserva.ImplementationUseCase;
using ModuloReservaContract.Dtos;
using Moq;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReservaTest
{
    public class CrearReservaUserCaseTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly Mock<IValidator<RequestCrearReserva>> _validatorMock;
        private readonly CrearReservaUserCase _useCase;

        public CrearReservaUserCaseTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _validatorMock = new Mock<IValidator<RequestCrearReserva>>();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RequestCrearReserva>(), default))
                .ReturnsAsync(new ValidationResult());

            _useCase = new CrearReservaUserCase(_context, _validatorMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            _context.Eventos.Add(new Evento
            {
                Id = 1,
                Titulo = "Concierto Rock",
                Descripcion = "Gran concierto de rock",
                CapacidadMaxima = 100,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                IniciaHora = new TimeOnly(20, 0),
                FinEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                FinHora = new TimeOnly(23, 0),
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });

            _context.Eventos.Add(new Evento
            {
                Id = 2,
                Titulo = "Evento Premium",
                Descripcion = "Evento con precio alto",
                CapacidadMaxima = 200,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                IniciaHora = new TimeOnly(20, 0),
                FinEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                FinHora = new TimeOnly(23, 0),
                Precio = 200m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });

            _context.Eventos.Add(new Evento
            {
                Id = 3,
                Titulo = "Evento Pronto",
                Descripcion = "Evento que inicia pronto",
                CapacidadMaxima = 100,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now),
                IniciaHora = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(30)),
                FinEvento = DateOnly.FromDateTime(DateTime.Now),
                FinHora = TimeOnly.FromDateTime(DateTime.Now.AddHours(3)),
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });

            _context.Eventos.Add(new Evento
            {
                Id = 4,
                Titulo = "Evento 12 Horas",
                Descripcion = "Evento que inicia en 12 horas",
                CapacidadMaxima = 100,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now.AddHours(12)),
                IniciaHora = TimeOnly.FromDateTime(DateTime.Now.AddHours(12)),
                FinEvento = DateOnly.FromDateTime(DateTime.Now.AddHours(15)),
                FinHora = TimeOnly.FromDateTime(DateTime.Now.AddHours(15)),
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            });

            _context.SaveChanges();
        }

        private static RequestCrearReserva CrearRequestValido() => new(
            EventoId: 1,
            Cantidad: 3,
            NombreComprador: "Juan Pérez",
            EmailComprado: "juan@email.com"
        );

        [Fact]
        public async Task ExecuteAsync_RequestValido_DebeCrearReserva()
        {
            var request = CrearRequestValido();

            await _useCase.ExecuteAsync(request);

            var reserva = await _context.Reservas.FirstOrDefaultAsync();
            reserva.Should().NotBeNull();
            reserva!.Cantidad.Should().Be(3);
            reserva.EstadoReservaId.Should().Be((int)EstadoReservaEnum.PendientePago);
        }

        [Fact]
        public async Task ExecuteAsync_ValidacionFalla_DebeLanzarValidationException()
        {
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RequestCrearReserva>(), default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("EventoId", "EventoId es requerido")
                }));

            var request = CrearRequestValido();
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ExecuteAsync_EventoNoExiste_DebeLanzarNotFoundCustomException()
        {
            var request = CrearRequestValido() with { EventoId = 999 };
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_ExcedeCapacidad_DebeLanzarEventoSoldOutException()
        {
            _context.Reservas.Add(new Reserva
            {
                Cantidad = 99,
                NombreComprador = "Existente",
                EmailComprador = "existente@mail.com",
                EventoId = 1,
                EstadoReservaId = (int)EstadoReservaEnum.Confirmada
            });
            await _context.SaveChangesAsync();

            var request = CrearRequestValido() with { Cantidad = 5 };
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<EventoSoldOutException>();
        }

        [Fact]
        public async Task ExecuteAsync_CapacidadExacta_DebeCrearReserva()
        {
            _context.Reservas.Add(new Reserva
            {
                Cantidad = 97,
                NombreComprador = "Existente",
                EmailComprador = "existente@mail.com",
                EventoId = 1,
                EstadoReservaId = (int)EstadoReservaEnum.Confirmada
            });
            await _context.SaveChangesAsync();

            var request = CrearRequestValido() with { Cantidad = 3 };

            await _useCase.ExecuteAsync(request);

            var total = await _context.Reservas.CountAsync();
            total.Should().Be(2);
        }

        [Fact]
        public async Task ExecuteAsync_EventoMenosDe1Hora_DebeLanzarReservaLimitException()
        {
            var request = CrearRequestValido() with { EventoId = 3, Cantidad = 2 };
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ReservaLimitException>()
                .WithMessage("*menos de 1 hora*");
        }

        [Fact]
        public async Task ExecuteAsync_MenosDe24HorasMasDe5Entradas_DebeLanzarReservaLimitException()
        {
            var request = CrearRequestValido() with { EventoId = 4, Cantidad = 6 };
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ReservaLimitException>()
                .WithMessage("*menos de 24 horas*");
        }

        [Fact]
        public async Task ExecuteAsync_MenosDe24Horas5Entradas_DebeCrearReserva()
        {
            var request = CrearRequestValido() with { EventoId = 4, Cantidad = 5 };

            await _useCase.ExecuteAsync(request);

            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.EventoId == 4);
            reserva.Should().NotBeNull();
            reserva!.Cantidad.Should().Be(5);
        }

        [Fact]
        public async Task ExecuteAsync_PrecioMayor100MasDe10Entradas_DebeLanzarReservaLimitException()
        {
            var request = CrearRequestValido() with { EventoId = 2, Cantidad = 11 };
            var act = () => _useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ReservaLimitException>()
                .WithMessage("*precio mayor a $100*");
        }

        [Fact]
        public async Task ExecuteAsync_PrecioMayor100Con10Entradas_DebeCrearReserva()
        {
            var request = CrearRequestValido() with { EventoId = 2, Cantidad = 10 };

            await _useCase.ExecuteAsync(request);

            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.EventoId == 2);
            reserva.Should().NotBeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
