using ModuloSecurityContract.Dtos;

namespace ModuloSecurityContract
{
    public interface ILoginUseCase
    {
        /// <summary>
        /// Autentica un usuario y retorna un token JWT.
        /// </summary>
        /// <param name="request">Credenciales del usuario.</param>
        /// <returns>Token JWT con información del usuario.</returns>
        Task<ResponseLogin> ExecuteAsync(RequestLogin request);
    }
}
