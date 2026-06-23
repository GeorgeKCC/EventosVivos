using Microsoft.AspNetCore.Mvc;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;

namespace EventosVivos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController(ICrearEventoUseCase crearEventoUseCase) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(RequestCrearEvento requestCrearEvento)
        {
            await crearEventoUseCase.ExecuteAsync(requestCrearEvento);
            return NoContent();
        }
    }
}
