using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModuloReserva.Strategies.StrategyImplementation;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReservaTest
{
    public class StrategyReservaCanceladaTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly StrategyReservaCancelada _strategy;

        public StrategyReservaCanceladaTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _strategy = new StrategyReservaCancelada(_context);

            SeedData();
        }

        private void SeedData()
        {
            var eventoLejano = new Evento
            {
                Id = 1,
                Titulo = "Evento Lejano",
                Descripcion = "Evento en 7 dias",
                CapacidadMaxima = 100,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                IniciaHora = new TimeOnly(20, 0),
                FinEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                FinHora = new TimeOnly(23, 0),
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            };

            var eventoCercano = new Evento
            {
                Id = 2,
                Titulo = "Evento Cercano",
                Descripcion = "Evento en 24 horas",
                CapacidadMaxima = 100,
                InicioEvento = DateOnly.FromDateTime(DateTime.Now.AddHours(24)),
                IniciaHora = TimeOnly.FromDateTime(DateTime.Now.AddHours(24)),
                FinEvento = DateOnly.FromDateTime(DateTime.Now.AddHours(27)),
                FinHora = TimeOnly.FromDateTime(DateTime.Now.AddHours(27)),
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = (int)EstadoEnum.Activo
            };

            _context.Eventos.AddRange(eventoLejano, eventoCercano);

            _context.Reservas.AddRange(
                new Reserva
                {
                    Id = 1,
                    Cantidad = 5,
                    NombreComprador = "Juan",
                    EmailComprador = "juan@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Confirmada
                },
                new Reserva
                {
                    Id = 2,
                    Cantidad = 3,
                    NombreComprador = "Maria",
                    EmailComprador = "maria@mail.com",
                    EventoId = 2,
                    EstadoReservaId = (int)EstadoReservaEnum.Confirmada
                },
                new Reserva
                {
                    Id = 3,
                    Cantidad = 2,
                    NombreComprador = "Pedro",
                    EmailComprador = "pedro@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Cancelada
                },
                new Reserva
                {
                    Id = 4,
                    Cantidad = 4,
                    NombreComprador = "Ana",
                    EmailComprador = "ana@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.PendientePago
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public void StrategyId_DebeSerCancelada()
        {
            _strategy.StrategyId.Should().Be((int)EstadoReservaEnum.Cancelada);
        }

        [Fact]
        public async Task ExecuteAsync_ReservaNoExiste_DebeLanzarNotFoundCustomException()
        {
            var act = () => _strategy.ExecuteAsync(999);
            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_ReservaYaCancelada_DebeLanzarReservaLimitException()
        {
            var act = () => _strategy.ExecuteAsync(3);
            await act.Should().ThrowAsync<ReservaLimitException>();
        }

        [Fact]
        public async Task ExecuteAsync_ReservaPendientePago_DebeLanzarReservaLimitException()
        {
            var act = () => _strategy.ExecuteAsync(4);
            await act.Should().ThrowAsync<ReservaLimitException>();
        }

        [Fact]
        public async Task ExecuteAsync_CancelarSinPenalizacion_DebeLibrarEntradas()
        {
            await _strategy.ExecuteAsync(1);

            var reserva = await _context.Reservas.FindAsync(1);
            reserva!.EstadoReservaId.Should().Be((int)EstadoReservaEnum.Cancelada);
            reserva.FechaCancelacion.Should().NotBeNull();
            reserva.Cantidad.Should().Be(0);
            reserva.EsPerdida.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteAsync_CancelarConPenalizacion_DebeMarcarComoPerdida()
        {
            await _strategy.ExecuteAsync(2);

            var reserva = await _context.Reservas.FindAsync(2);
            reserva!.EstadoReservaId.Should().Be((int)EstadoReservaEnum.Cancelada);
            reserva.FechaCancelacion.Should().NotBeNull();
            reserva.EsPerdida.Should().BeTrue();
            reserva.Cantidad.Should().Be(3);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
