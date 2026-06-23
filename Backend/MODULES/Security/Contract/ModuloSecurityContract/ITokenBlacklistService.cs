namespace ModuloSecurityContract
{
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Agrega un token a la lista negra.
        /// </summary>
        Task RevokeTokenAsync(string token);

        /// <summary>
        /// Verifica si un token está revocado.
        /// </summary>
        Task<bool> IsTokenRevokedAsync(string token);
    }
}
