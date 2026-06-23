using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ModuloReporteContract;
using ModuloReporteContract.Dtos;

namespace EventosVivos.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ReporteController(IGenerarReporteUseCase generarReporteUseCase) : ControllerBase
    {
        /// <summary>
        /// Descarga un reporte en formato Excel según el tipo de reporte solicitado.
        /// </summary>
        /// <param name="requestGenerateReporte">Datos con el evento y tipo de reporte a generar.</param>
        /// <returns>Archivo Excel con el reporte generado.</returns>
        [HttpGet]
        public async Task<IActionResult> DownloadReport([FromQuery] RequestGenerateReporte requestGenerateReporte)
        {
            var result = await generarReporteUseCase.ExecuteAsync(requestGenerateReporte);

            return File(result.FileContent, result.ContentType, result.FileName);
        }
    }
}
