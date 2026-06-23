using Microsoft.AspNetCore.Mvc;
using ModuloReporteContract;
using ModuloReporteContract.Dtos;

namespace EventosVivos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController(IGenerarReporteUseCase generarReporteUseCase) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> DownloadReport([FromQuery] RequestGenerateReporte requestGenerateReporte)
        {
            var result = await generarReporteUseCase.ExecuteAsync(requestGenerateReporte);

            return File(result.FileContent, result.ContentType, result.FileName);
        }
    }
}
