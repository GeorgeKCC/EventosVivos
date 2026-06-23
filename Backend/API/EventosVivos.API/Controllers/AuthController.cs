using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ModuloSecurityContract;
using ModuloSecurityContract.Dtos;

namespace EventosVivos.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController(ILoginUseCase loginUseCase) : ControllerBase
    {
        /// <summary>
        /// Autentica un usuario y retorna un token JWT.
        /// </summary>
        /// <param name="request">Credenciales del usuario.</param>
        /// <returns>Token JWT.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(RequestLogin request)
        {
            var result = await loginUseCase.ExecuteAsync(request);
            return Ok(result);
        }
    }
}
