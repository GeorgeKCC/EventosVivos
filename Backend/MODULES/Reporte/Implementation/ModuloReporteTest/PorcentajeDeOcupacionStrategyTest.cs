using ClosedXML.Excel;
using FluentAssertions;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class PorcentajeDeOcupacionStrategyTest : IDisposable
    {
        private readonly EventosVivosDbContext _context;
        private readonly PorcentajeDeOcupaciónStrategy _strategy;

        public PorcentajeDeOcupacionStrategyTest()
        {
            _context = ReporteTestHelper.CreateContext();
            ReporteTestHelper.SeedData(_context);
            _strategy = new PorcentajeDeOcupaciónStrategy(_context);
        }

        [Fact]
        public void TipoReporteId_DebeSerPorcentajeDeOcupacion()
        {
            _strategy.TipoReporteId.Should().Be((int)TipoReporte.PorcentajeDeOcupacion);
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
            result.FileName.Should().Contain("PorcentajeOcupacion_Evento_1");
        }

        [Fact]
        public async Task ExecuteAsync_DebeCalcularPorcentajeCorrectamente()
        {
            var result = await _strategy.ExecuteAsync(1);

            using var stream = new MemoryStream(result.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Confirmadas sin perdidas: 5 + 3 = 8
            var confirmadas = worksheet.Cell(9, 2).GetValue<int>();
            confirmadas.Should().Be(8);

            // Pendientes sin perdidas: 2
            var pendientes = worksheet.Cell(10, 2).GetValue<int>();
            pendientes.Should().Be(2);

            // Perdidas: 4
            var perdidas = worksheet.Cell(11, 2).GetValue<int>();
            perdidas.Should().Be(4);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
