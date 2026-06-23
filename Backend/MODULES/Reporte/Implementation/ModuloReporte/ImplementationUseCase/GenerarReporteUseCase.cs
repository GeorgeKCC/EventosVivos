using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract;
using ModuloReporteContract.Dtos;
using Transversal.Exceptions.Customs;

namespace ModuloReporte.ImplementationUseCase
{
    internal class GenerarReporteUseCase(IEnumerable<IStrategyReporte> strategyReportes) : IGenerarReporteUseCase
    {
        public async Task<ReporteResult> ExecuteAsync(RequestGenerateReporte requestGenerateReporte)
        {
            var strategy = strategyReportes.FirstOrDefault(s => s.TipoReporteId == (int)requestGenerateReporte.TipoReporte) ??
                           throw new NotFoundCustomException("No se encontró una estrategia para el tipo de reporte especificado.");

            return await strategy.ExecuteAsync(requestGenerateReporte.EventoId);
        }
    }
}
