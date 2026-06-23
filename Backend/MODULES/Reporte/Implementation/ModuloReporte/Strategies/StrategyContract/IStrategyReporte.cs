using ModuloReporteContract.Dtos;

namespace ModuloReporte.Strategies.StrategyContract
{
    internal interface IStrategyReporte
    {
        int TipoReporteId { get; }

        Task<ReporteResult> ExecuteAsync(int EventoId);
    }
}
