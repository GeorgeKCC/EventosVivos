using ModuloSecurityContract;
using System.Collections.Concurrent;

namespace ModuloSecurity.ImplementationUseCase
{
    /// <summary>
    /// Servicio en memoria para gestionar tokens JWT revocados.
    /// </summary>
    internal class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

        /// <summary>
        /// Agrega un token a la lista negra para invalidarlo.
        /// </summary>
        /// <param name="token">Token JWT a revocar.</param>
        public Task RevokeTokenAsync(string token)
        {
            _revokedTokens.TryAdd(token, DateTime.UtcNow);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Verifica si un token ha sido revocado.
        /// </summary>
        /// <param name="token">Token JWT a verificar.</param>
        /// <returns>True si el token está revocado, false en caso contrario.</returns>
        public Task<bool> IsTokenRevokedAsync(string token)
        {
            return Task.FromResult(_revokedTokens.ContainsKey(token));
        }
    }
}
