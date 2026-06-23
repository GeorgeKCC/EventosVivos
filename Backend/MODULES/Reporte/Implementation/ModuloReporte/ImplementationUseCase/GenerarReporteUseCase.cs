using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract;
using ModuloReporteContract.Dtos;
using Transversal.Exceptions.Customs;

namespace ModuloReporte.ImplementationUseCase
{
    internal class GenerarReporteUseCase(IEnumerable<IStrategyReporte> strategyReportes) : IGenerarReporteUseCase
    {
        /// <summary>
        /// Genera un reporte delegando a la estrategia correspondiente según el tipo de reporte.
        /// </summary>
        /// <param name="requestGenerateReporte">Datos con el evento y tipo de reporte a generar.</param>
        /// <returns>Resultado del reporte con el contenido del archivo y nombre.</returns>
        public async Task<ReporteResult> ExecuteAsync(RequestGenerateReporte requestGenerateReporte)
        {
            var strategy = strategyReportes.FirstOrDefault(s => s.TipoReporteId == (int)requestGenerateReporte.TipoReporte) ??
                           throw new NotFoundCustomException("No se encontró una estrategia para el tipo de reporte especificado.");

            return await strategy.ExecuteAsync(requestGenerateReporte.EventoId);
        }
    }
}
