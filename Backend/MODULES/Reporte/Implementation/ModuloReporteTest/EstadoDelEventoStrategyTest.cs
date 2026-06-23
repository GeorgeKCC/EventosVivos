using ClosedXML.Excel;
using FluentAssertions;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class EstadoDelEventoStrategyTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly EstadoDelEventoStrategy _strategy;

        public EstadoDelEventoStrategyTest()
        {
            _context = ReporteTestHelper.CreateContext();
            ReporteTestHelper.SeedData(_context);
            _strategy = new EstadoDelEventoStrategy(_context);
        }

        [Fact]
        public void TipoReporteId_DebeSerEstadoDelEvento()
        {
            _strategy.TipoReporteId.Should().Be((int)TipoReporte.EstadoDelEvento);
        }

        [Fact]
        public async Task ExecuteAsync_EventoNoExiste_DebeLanzarNotFoundCustomException()
        {
            var act = () => _strategy.ExecuteAsync(999);
            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_EventoValido_DebeRetornarExcel()
        {
            var result = await _strategy.ExecuteAsync(1);

            result.Should().NotBeNull();
            result.FileContent.Should().NotBeEmpty();
            result.FileName.Should().Contain("EstadoEvento_Evento_1");
        }

        [Fact]
        public async Task ExecuteAsync_DebeContenerDatosDelEvento()
        {
            var result = await _strategy.ExecuteAsync(1);

            using var stream = new MemoryStream(result.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            worksheet.Cell(2, 2).GetValue<string>().Should().Be("Concierto Rock");
            worksheet.Cell(4, 2).GetValue<string>().Should().Be("Activo");
            worksheet.Cell(6, 2).GetValue<string>().Should().Be("Arena CDMX");
            worksheet.Cell(13, 2).GetValue<int>().Should().Be(100);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
