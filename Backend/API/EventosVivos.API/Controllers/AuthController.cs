using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuloSecurityContract;
using ModuloSecurityContract.Dtos;

namespace EventosVivos.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController(ILoginUseCase loginUseCase, ILogoutUseCase logoutUseCase) : ControllerBase
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

        /// <summary>
        /// Cierra la sesión del usuario invalidando su token JWT.
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            await logoutUseCase.ExecuteAsync(token);
            return Ok(new { message = "Sesión cerrada correctamente." });
        }
    }
}
