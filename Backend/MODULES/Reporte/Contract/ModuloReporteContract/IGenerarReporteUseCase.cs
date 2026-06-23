using ModuloReporteContract.Dtos;

namespace ModuloReporteContract
{
    public interface IGenerarReporteUseCase
    {
        /// <summary>
        /// Genera un reporte en formato Excel según el tipo de reporte solicitado.
        /// </summary>
        /// <param name="requestGenerateReporte">Datos con el evento y tipo de reporte a generar.</param>
        /// <returns>Resultado del reporte con el contenido del archivo y nombre.</returns>
        Task<ReporteResult> ExecuteAsync(RequestGenerateReporte requestGenerateReporte);
    }
}
