using ClosedXML.Excel;
using FluentAssertions;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class TotalEntradasVendidasStrategyTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly TotalEntradasVendidasStrategy _strategy;

        public TotalEntradasVendidasStrategyTest()
        {
            _context = ReporteTestHelper.CreateContext();
            ReporteTestHelper.SeedData(_context);
            _strategy = new TotalEntradasVendidasStrategy(_context);
        }

        [Fact]
        public void TipoReporteId_DebeSerTotalEntradas()
        {
            _strategy.TipoReporteId.Should().Be((int)TipoReporte.TotalEntradas);
        }

        [Fact]
        public async Task ExecuteAsync_EventoNoExiste_DebeLanzarNotFoundCustomException()
        {
            var act = () => _strategy.ExecuteAsync(999);
            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_EventoValido_DebeRetornarExcelConDatos()
        {
            var result = await _strategy.ExecuteAsync(1);

            result.Should().NotBeNull();
            result.FileContent.Should().NotBeEmpty();
            result.FileName.Should().Contain("EntradasVendidas_Evento_1");
            result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public async Task ExecuteAsync_DebeContenerSoloReservasConfirmadas()
        {
            var result = await _strategy.ExecuteAsync(1);

            using var stream = new MemoryStream(result.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            worksheet.Cell(4, 2).GetValue<int>().Should().Be(8); // 5 + 3 confirmadas
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
