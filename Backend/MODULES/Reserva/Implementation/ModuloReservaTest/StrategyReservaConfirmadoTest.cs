using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModuloReserva.Strategies.StrategyImplementation;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReservaTest
{
    public class StrategyReservaConfirmadoTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly StrategyReservaConfirmado _strategy;

        public StrategyReservaConfirmadoTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _strategy = new StrategyReservaConfirmado(_context);

            SeedData();
        }

        private void SeedData()
        {
            _context.Reservas.AddRange(
                new Reserva
                {
                    Id = 1,
                    Cantidad = 3,
                    NombreComprador = "Juan",
                    EmailComprador = "juan@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.PendientePago
                },
                new Reserva
                {
                    Id = 2,
                    Cantidad = 2,
                    NombreComprador = "Maria",
                    EmailComprador = "maria@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Confirmada
                },
                new Reserva
                {
                    Id = 3,
                    Cantidad = 4,
                    NombreComprador = "Pedro",
                    EmailComprador = "pedro@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Cancelada
                }
            );
            _context.SaveChanges();
        }

        [Fact]
        public void StrategyId_DebeSerConfirmada()
        {
            _strategy.StrategyId.Should().Be((int)EstadoReservaEnum.Confirmada);
        }

        [Fact]
        public async Task ExecuteAsync_ReservaPendiente_DebeConfirmar()
        {
            await _strategy.ExecuteAsync(1);

            var reserva = await _context.Reservas.FindAsync(1);
            reserva!.EstadoReservaId.Should().Be((int)EstadoReservaEnum.Confirmada);
        }

        [Fact]
        public async Task ExecuteAsync_ReservaNoExiste_DebeLanzarNotFoundCustomException()
        {
            var act = () => _strategy.ExecuteAsync(999);
            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_ReservaYaConfirmada_DebeLanzarReservationConfirmedException()
        {
            var act = () => _strategy.ExecuteAsync(2);
            await act.Should().ThrowAsync<ReservationConfirmedException>();
        }

        [Fact]
        public async Task ExecuteAsync_ReservaCancelada_DebeLanzarReservationCancelException()
        {
            var act = () => _strategy.ExecuteAsync(3);
            await act.Should().ThrowAsync<ReservationCancelException>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
