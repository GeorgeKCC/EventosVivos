using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract.Dtos;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloEventoTest
{
    public class BuscarEventoUseCaseTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly BuscarEventoUseCase _useCase;

        public BuscarEventoUseCaseTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _useCase = new BuscarEventoUseCase(_context);

            SeedData();
        }

        private void SeedData()
        {
            _context.Venues.AddRange(
                new Venue { Id = 1, Nombre = "Arena CDMX", Capacidad = 200, Ciudad = "CDMX" },
                new Venue { Id = 2, Nombre = "Foro Sol", Capacidad = 500, Ciudad = "CDMX" }
            );

            _context.TipoEventos.AddRange(
                new TipoEvento { Id = 1, Nombre = "Conferencia" },
                new TipoEvento { Id = 2, Nombre = "Concierto" }
            );

            _context.EstadoEvento.AddRange(
                new EstadoEvento { Id = 1, Nombre = "Activo" },
                new EstadoEvento { Id = 2, Nombre = "Completado" }
            );

            _context.Eventos.AddRange(
                new Evento
                {
                    Id = 1,
                    Titulo = "Conferencia Tech",
                    Descripcion = "Conferencia de tecnologia",
                    CapacidadMaxima = 100,
                    InicioEvento = new DateOnly(2025, 7, 10),
                    IniciaHora = new TimeOnly(9, 0),
                    FinEvento = new DateOnly(2025, 7, 10),
                    FinHora = new TimeOnly(17, 0),
                    Precio = 50m,
                    VenueId = 1,
                    TipoEventoId = 1,
                    EstadoId = 1
                },
                new Evento
                {
                    Id = 2,
                    Titulo = "Concierto Rock",
                    Descripcion = "Gran concierto de rock",
                    CapacidadMaxima = 300,
                    InicioEvento = new DateOnly(2025, 8, 15),
                    IniciaHora = new TimeOnly(20, 0),
                    FinEvento = new DateOnly(2025, 8, 15),
                    FinHora = new TimeOnly(23, 0),
                    Precio = 200m,
                    VenueId = 2,
                    TipoEventoId = 2,
                    EstadoId = 1
                },
                new Evento
                {
                    Id = 3,
                    Titulo = "Conferencia IA",
                    Descripcion = "Conferencia de inteligencia artificial",
                    CapacidadMaxima = 150,
                    InicioEvento = new DateOnly(2025, 9, 20),
                    IniciaHora = new TimeOnly(10, 0),
                    FinEvento = new DateOnly(2025, 9, 20),
                    FinHora = new TimeOnly(18, 0),
                    Precio = 80m,
                    VenueId = 1,
                    TipoEventoId = 1,
                    EstadoId = 2
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task ExecuteAsync_SinFiltros_DebeRetornarTodosLosEventos()
        {
            var request = new RequestBuscarEvento(null, null, null, null, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorTipoEvento_DebeRetornarSoloEseTipo()
        {
            var request = new RequestBuscarEvento(TipoEventoId: 1, null, null, null, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.TipoEventoId == 1);
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorVenue_DebeRetornarSoloEseVenue()
        {
            var request = new RequestBuscarEvento(null, null, null, VenueId: 2, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(1);
            result.First().VenueName.Should().Be("Foro Sol");
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorEstado_DebeRetornarSoloEseEstado()
        {
            var request = new RequestBuscarEvento(null, null, null, null, EstadoId: 2, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(1);
            result.First().Titulo.Should().Be("Conferencia IA");
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorTitulo_DebeRetornarCoincidencias()
        {
            var request = new RequestBuscarEvento(null, null, null, null, null, Titulo: "Conferencia");

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.Titulo.Contains("Conferencia"));
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorRangoFechas_DebeRetornarEventosEnRango()
        {
            var request = new RequestBuscarEvento(
                null,
                FechaInicioEventoRange: new DateOnly(2025, 7, 1),
                FechaFinEventoRange: new DateOnly(2025, 8, 31),
                null, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorFechaInicio_DebeRetornarDesdeEsaFecha()
        {
            var request = new RequestBuscarEvento(
                null,
                FechaInicioEventoRange: new DateOnly(2025, 8, 1),
                null, null, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExecuteAsync_FiltrarPorFechaFin_DebeRetornarHastaEsaFecha()
        {
            var request = new RequestBuscarEvento(
                null, null,
                FechaFinEventoRange: new DateOnly(2025, 7, 31),
                null, null, null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(1);
            result.First().Titulo.Should().Be("Conferencia Tech");
        }

        [Fact]
        public async Task ExecuteAsync_MultiplesFiltros_DebeAplicarTodos()
        {
            var request = new RequestBuscarEvento(
                TipoEventoId: 1,
                null, null,
                VenueId: 1,
                EstadoId: 1,
                null);

            var result = await _useCase.ExecuteAsync(request);

            result.Should().HaveCount(1);
            result.First().Titulo.Should().Be("Conferencia Tech");
        }

        [Fact]
        public async Task ExecuteAsync_FiltroSinResultados_DebeRetornarListaVacia()
        {
            var request = new RequestBuscarEvento(null, null, null, null, null, Titulo: "NoExiste");

            var result = await _useCase.ExecuteAsync(request);

            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
