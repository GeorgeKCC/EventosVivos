using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using Transversal.Cache.Attributes;

namespace EventosVivos.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class EventoController(ICrearEventoUseCase crearEventoUseCase,
                                  IBuscarEventoUseCase buscarEventoUseCase) : ControllerBase
    {
        /// <summary>
        /// Crea un nuevo evento.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a crear.</param>
        /// <returns></returns>
        [HttpPost]
        [InvalidateCache(Tags = new[] { "Evento" })]
        public async Task<IActionResult> Create(RequestCrearEvento requestCrearEvento)
        {
            await crearEventoUseCase.ExecuteAsync(requestCrearEvento);
            return NoContent();
        }

        /// <summary>
        /// Busca eventos según los filtros proporcionados.
        /// </summary>
        /// <param name="requestBuscarEvento">Filtros de búsqueda para eventos.</param>
        /// <returns>Lista de eventos que coinciden con los filtros.</returns>
        [HttpGet]
        [Cacheable("Evento:list", DurationSeconds = 120, Tags = new[] { "Evento" })]
        public async Task<IActionResult> Search([FromQuery] RequestBuscarEvento requestBuscarEvento)
        {
           var result = await buscarEventoUseCase.ExecuteAsync(requestBuscarEvento);
            return Ok(result);
        }
    }
}
