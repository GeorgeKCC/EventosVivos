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
                                    IConfirmarReservaUseCase confirmarReservaUseCase,
                                    IObtenerTodoReservas obtenerTodoReservas) : ControllerBase
    {

        /// <summary>
        /// Crea una nueva reserva para un evento.
        /// </summary>
        /// <param name="requestCrearReserva">Datos de la reserva a crear.</param>
        /// <returns></returns>
        [HttpPost]
        [InvalidateCache(Tags = new[] { "Reserva" })]
        public async Task<IActionResult> Create(RequestCrearReserva requestCrearReserva)
        {
            await crearReservaUserCase.ExecuteAsync(requestCrearReserva);
            return NoContent();
        }

        /// <summary>
        /// Actualiza el estado de una reserva (confirmar o cancelar).
        /// </summary>
        /// <param name="requestEstadoReserva">Datos con el nuevo estado de la reserva.</param>
        /// <returns></returns>
        [HttpPost("Payment")]
        [InvalidateCache(Tags = new[] { "Reserva" })]
        public async Task<IActionResult> Payment(RequestEstadoReserva requestEstadoReserva)
        {
            var result = await confirmarReservaUseCase.ExecuteAsync(requestEstadoReserva);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todas las reservas registradas.
        /// </summary>
        /// <returns>Lista de reservas.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await obtenerTodoReservas.ExecuteAsync();
            return Ok(result);
        }
    }
}
