using Microsoft.AspNetCore.Mvc;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;
using Transversal.Cache.Attributes;

namespace EventosVivos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController(ICrearReservaUserCase crearReservaUserCase) : ControllerBase
    {

        [HttpPost]
        [InvalidateCache(Tags = new[] { "Reserva" })]
        public async Task<IActionResult> Create(RequestCrearReserva requestCrearReserva)
        {
            await crearReservaUserCase.ExecuteAsync(requestCrearReserva);
            return NoContent();
        }
    }
}
