using ClosedXML.Excel;
using FluentAssertions;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class TotalIngresosStrategyTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly TotalIngresosStrategy _strategy;

        public TotalIngresosStrategyTest()
        {
            _context = ReporteTestHelper.CreateContext();
            ReporteTestHelper.SeedData(_context);
            _strategy = new TotalIngresosStrategy(_context);
        }

        [Fact]
        public void TipoReporteId_DebeSerTotalDeIngresos()
        {
            _strategy.TipoReporteId.Should().Be((int)TipoReporte.TotalDeIngresos);
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
            result.FileName.Should().Contain("TotalIngresos_Evento_1");
        }

        [Fact]
        public async Task ExecuteAsync_DebeCalcularIngresosCorrectamente()
        {
            var result = await _strategy.ExecuteAsync(1);

            using var stream = new MemoryStream(result.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Precio: 150, confirmadas: 5 + 3 = 8, total: 8 * 150 = 1200
            var totalEntradas = worksheet.Cell(6, 2).GetValue<int>();
            totalEntradas.Should().Be(8);

            var totalIngresos = worksheet.Cell(7, 2).GetValue<decimal>();
            totalIngresos.Should().Be(1200m);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
