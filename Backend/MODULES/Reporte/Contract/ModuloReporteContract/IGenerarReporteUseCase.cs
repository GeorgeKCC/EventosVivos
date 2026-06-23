using ModuloReporteContract.Dtos;

namespace ModuloReporteContract
{
    public interface IGenerarReporteUseCase
    {
        Task<ReporteResult> ExecuteAsync(RequestGenerateReporte requestGenerateReporte);
    }
}
