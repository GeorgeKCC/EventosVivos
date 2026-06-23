using ModuloSecurityContract;

namespace ModuloSecurity.ImplementationUseCase
{
    /// <summary>
    /// Caso de uso para cerrar sesión invalidando el token JWT del usuario.
    /// </summary>
    internal class LogoutUseCase(ITokenBlacklistService tokenBlacklistService) : ILogoutUseCase
    {
        /// <summary>
        /// Invalida el token JWT proporcionado agregándolo a la lista negra.
        /// </summary>
        /// <param name="token">Token JWT a invalidar.</param>
        /// <exception cref="UnauthorizedAccessException">Se lanza si el token es nulo o vacío.</exception>
        public async Task ExecuteAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException("Token no proporcionado.");
            }

            await tokenBlacklistService.RevokeTokenAsync(token);
        }
    }
}
