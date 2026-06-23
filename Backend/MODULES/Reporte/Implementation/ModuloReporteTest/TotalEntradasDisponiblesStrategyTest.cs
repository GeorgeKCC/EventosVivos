using ClosedXML.Excel;
using FluentAssertions;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class TotalEntradasDisponiblesStrategyTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly TotalEntradasDisponiblesStrategy _strategy;

        public TotalEntradasDisponiblesStrategyTest()
        {
            _context = ReporteTestHelper.CreateContext();
            ReporteTestHelper.SeedData(_context);
            _strategy = new TotalEntradasDisponiblesStrategy(_context);
        }

        [Fact]
        public void TipoReporteId_DebeSerTotalEntradasDisponibles()
        {
            _strategy.TipoReporteId.Should().Be((int)TipoReporte.TotalEntradasDisponibles);
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
            result.FileName.Should().Contain("EntradasDisponibles_Evento_1");
        }

        [Fact]
        public async Task ExecuteAsync_DebeExcluirReservasPerdidas()
        {
            var result = await _strategy.ExecuteAsync(1);

            using var stream = new MemoryStream(result.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Total reservadas sin perdidas: 5 + 3 + 2 + 0 = 10 (excluye la perdida de 4)
            var totalReservadas = worksheet.Cell(7, 2).GetValue<int>();
            totalReservadas.Should().Be(10);

            // Disponibles: 100 - 10 = 90
            var disponibles = worksheet.Cell(8, 2).GetValue<int>();
            disponibles.Should().Be(90);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
