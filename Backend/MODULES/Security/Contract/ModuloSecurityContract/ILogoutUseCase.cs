namespace ModuloSecurityContract
{
    public interface ILogoutUseCase
    {
        /// <summary>
        /// Cierra la sesión del usuario invalidando su token.
        /// </summary>
        /// <param name="token">Token JWT a invalidar.</param>
        Task ExecuteAsync(string token);
    }
}
