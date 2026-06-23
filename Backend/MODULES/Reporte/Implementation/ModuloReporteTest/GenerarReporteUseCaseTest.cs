using FluentAssertions;
using ModuloReporte.ImplementationUseCase;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract.Dtos;
using ModuloReporteContract.Enums;
using Moq;
using Transversal.Exceptions.Customs;

namespace ModuloReporteTest
{
    public class GenerarReporteUseCaseTest
    {
        [Fact]
        public async Task ExecuteAsync_TipoReporteValido_DebeRetornarReporteResult()
        {
            var mockStrategy = new Mock<IStrategyReporte>();
            mockStrategy.Setup(s => s.TipoReporteId).Returns((int)TipoReporte.TotalEntradas);
            mockStrategy.Setup(s => s.ExecuteAsync(1))
                .ReturnsAsync(new ReporteResult(new byte[] { 1, 2, 3 }, "test.xlsx"));

            var useCase = new GenerarReporteUseCase(new[] { mockStrategy.Object });
            var request = new RequestGenerateReporte(1, TipoReporte.TotalEntradas);

            var result = await useCase.ExecuteAsync(request);

            result.Should().NotBeNull();
            result.FileContent.Should().NotBeEmpty();
            result.FileName.Should().Be("test.xlsx");
        }

        [Fact]
        public async Task ExecuteAsync_TipoReporteNoExiste_DebeLanzarNotFoundCustomException()
        {
            var mockStrategy = new Mock<IStrategyReporte>();
            mockStrategy.Setup(s => s.TipoReporteId).Returns((int)TipoReporte.TotalEntradas);

            var useCase = new GenerarReporteUseCase(new[] { mockStrategy.Object });
            var request = new RequestGenerateReporte(1, (TipoReporte)99);

            var act = () => useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_SinEstrategias_DebeLanzarNotFoundCustomException()
        {
            var useCase = new GenerarReporteUseCase(Enumerable.Empty<IStrategyReporte>());
            var request = new RequestGenerateReporte(1, TipoReporte.TotalEntradas);

            var act = () => useCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<NotFoundCustomException>();
        }

        [Fact]
        public async Task ExecuteAsync_DebeInvocarStrategyCorrecta()
        {
            var mockStrategy1 = new Mock<IStrategyReporte>();
            mockStrategy1.Setup(s => s.TipoReporteId).Returns((int)TipoReporte.TotalEntradas);
            mockStrategy1.Setup(s => s.ExecuteAsync(1))
                .ReturnsAsync(new ReporteResult(new byte[] { 1 }, "entradas.xlsx"));

            var mockStrategy2 = new Mock<IStrategyReporte>();
            mockStrategy2.Setup(s => s.TipoReporteId).Returns((int)TipoReporte.TotalDeIngresos);
            mockStrategy2.Setup(s => s.ExecuteAsync(1))
                .ReturnsAsync(new ReporteResult(new byte[] { 2 }, "ingresos.xlsx"));

            var useCase = new GenerarReporteUseCase(new[] { mockStrategy1.Object, mockStrategy2.Object });
            var request = new RequestGenerateReporte(1, TipoReporte.TotalDeIngresos);

            var result = await useCase.ExecuteAsync(request);

            result.FileName.Should().Be("ingresos.xlsx");
            mockStrategy2.Verify(s => s.ExecuteAsync(1), Times.Once);
            mockStrategy1.Verify(s => s.ExecuteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
