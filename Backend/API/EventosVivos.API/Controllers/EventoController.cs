using Microsoft.AspNetCore.Mvc;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using Transversal.Cache.Attributes;

namespace EventosVivos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController(ICrearEventoUseCase crearEventoUseCase,
                                  IBuscarEventoUseCase buscarEventoUseCase) : ControllerBase
    {
        [HttpPost]
        [InvalidateCache(Tags = new[] { "Evento" })]
        public async Task<IActionResult> Create(RequestCrearEvento requestCrearEvento)
        {
            await crearEventoUseCase.ExecuteAsync(requestCrearEvento);
            return NoContent();
        }

        [HttpGet]
        [Cacheable("Evento:list", DurationSeconds = 120, Tags = new[] { "Evento" })]
        public async Task<IActionResult> Search([FromQuery] RequestBuscarEvento requestBuscarEvento)
        {
           var result = await buscarEventoUseCase.ExecuteAsync(requestBuscarEvento);
            return Ok(result);
        }
    }
}
