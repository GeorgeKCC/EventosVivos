using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModuloTarea.Jobs;
using Moq;
using Quartz;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloTareaTest
{
    public class CompletarEventosJobTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly Mock<ILogger<CompletarEventosJob>> _loggerMock;
        private readonly CompletarEventosJob _job;
        private readonly Mock<IJobExecutionContext> _jobContextMock;

        public CompletarEventosJobTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventosVivosDbContext(options);
            _loggerMock = new Mock<ILogger<CompletarEventosJob>>();
            _job = new CompletarEventosJob(_context, _loggerMock.Object);
            _jobContextMock = new Mock<IJobExecutionContext>();
            _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        }

        private Evento CrearEvento(int id, string titulo, DateOnly finEvento, TimeOnly finHora, int estadoId)
        {
            return new Evento
            {
                Id = id,
                Titulo = titulo,
                Descripcion = "Descripción de prueba",
                CapacidadMaxima = 100,
                InicioEvento = new DateOnly(2024, 1, 1),
                IniciaHora = new TimeOnly(10, 0),
                FinEvento = finEvento,
                FinHora = finHora,
                Precio = 50m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = estadoId
            };
        }

        [Fact]
        public async Task Execute_EventoFinalizadoActivo_DebeMarcaComoCompletado()
        {
            var eventoFinalizado = CrearEvento(
                id: 1,
                titulo: "Evento Finalizado",
                finEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                finHora: new TimeOnly(10, 0),
                estadoId: (int)EstadoEnum.Activo);

            _context.Eventos.Add(eventoFinalizado);
            await _context.SaveChangesAsync();

            await _job.Execute(_jobContextMock.Object);

            var evento = await _context.Eventos.FindAsync(1);
            evento!.EstadoId.Should().Be((int)EstadoEnum.Completado);
        }

        [Fact]
        public async Task Execute_EventoAunNoFinalizado_DebePermancerActivo()
        {
            var eventoFuturo = CrearEvento(
                id: 2,
                titulo: "Evento Futuro",
                finEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                finHora: new TimeOnly(23, 0),
                estadoId: (int)EstadoEnum.Activo);

            _context.Eventos.Add(eventoFuturo);
            await _context.SaveChangesAsync();

            await _job.Execute(_jobContextMock.Object);

            var evento = await _context.Eventos.FindAsync(2);
            evento!.EstadoId.Should().Be((int)EstadoEnum.Activo);
        }

        [Fact]
        public async Task Execute_EventoYaCompletado_NoDebeModificarse()
        {
            var eventoCompletado = CrearEvento(
                id: 3,
                titulo: "Evento Ya Completado",
                finEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                finHora: new TimeOnly(10, 0),
                estadoId: (int)EstadoEnum.Completado);

            _context.Eventos.Add(eventoCompletado);
            await _context.SaveChangesAsync();

            await _job.Execute(_jobContextMock.Object);

            var evento = await _context.Eventos.FindAsync(3);
            evento!.EstadoId.Should().Be((int)EstadoEnum.Completado);
        }

        [Fact]
        public async Task Execute_SinEventosPendientes_NoDebeLanzarExcepcion()
        {
            var act = async () => await _job.Execute(_jobContextMock.Object);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Execute_MultiplesEventosFinalizados_DebeCompletarTodos()
        {
            var evento1 = CrearEvento(
                id: 4,
                titulo: "Evento Finalizado 1",
                finEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                finHora: new TimeOnly(10, 0),
                estadoId: (int)EstadoEnum.Activo);

            var evento2 = CrearEvento(
                id: 5,
                titulo: "Evento Finalizado 2",
                finEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                finHora: new TimeOnly(15, 0),
                estadoId: (int)EstadoEnum.Activo);

            _context.Eventos.AddRange(evento1, evento2);
            await _context.SaveChangesAsync();

            await _job.Execute(_jobContextMock.Object);

            var eventos = await _context.Eventos.ToListAsync();
            eventos.Should().AllSatisfy(e => e.EstadoId.Should().Be((int)EstadoEnum.Completado));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
