namespace EventosVivos.API.Middlewares
{
    /// <summary>
    /// Middleware que valida que cada request incluya un API Key válido en el header "X-Api-Key".
    /// </summary>
    public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Excluir rutas públicas: login, Scalar y OpenAPI
            if (path?.Contains("/Auth/login", StringComparison.OrdinalIgnoreCase) == true ||
                path?.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase) == true ||
                path?.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) == true)
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "API Key es requerida. Envíela en el header 'X-Api-Key'." });
                return;
            }

            var configuredApiKey = configuration["ApiKey"];

            if (string.IsNullOrEmpty(configuredApiKey) || !string.Equals(configuredApiKey, extractedApiKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "API Key inválida." });
                return;
            }

            await next(context);
        }
    }
}
