using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;
using Transversal.Cache.Attributes;

namespace EventosVivos.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ReservasController(ICrearReservaUserCase crearReservaUserCase,
                                    IConfirmarReservaUseCase confirmarReservaUseCase) : ControllerBase
    {

        [HttpPost]
        [InvalidateCache(Tags = new[] { "Reserva" })]
        public async Task<IActionResult> Create(RequestCrearReserva requestCrearReserva)
        {
            await crearReservaUserCase.ExecuteAsync(requestCrearReserva);
            return NoContent();
        }

        [HttpPost("Payment")]
        [InvalidateCache(Tags = new[] { "Reserva" })]
        public async Task<IActionResult> Payment(RequestEstadoReserva requestEstadoReserva)
        {
            await confirmarReservaUseCase.ExecuteAsync(requestEstadoReserva);
            return NoContent();
        }
    }
}
