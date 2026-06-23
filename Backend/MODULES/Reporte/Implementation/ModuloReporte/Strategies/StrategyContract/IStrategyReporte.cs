using ModuloReporteContract.Dtos;

namespace ModuloReporte.Strategies.StrategyContract
{
    internal interface IStrategyReporte
    {
        int TipoReporteId { get; }

        /// <summary>
        /// Genera el reporte Excel para un evento específico.
        /// </summary>
        /// <param name="EventoId">Identificador del evento.</param>
        /// <returns>Resultado del reporte con el contenido del archivo.</returns>
        Task<ReporteResult> ExecuteAsync(int EventoId);
    }
}
