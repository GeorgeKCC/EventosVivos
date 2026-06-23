using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Transversal.Exceptions.Handler;

namespace Transversal.Exceptions
{
    public static class ExceptionRegisterServices
    {
        /// <summary>
        /// Registra el manejador de excepciones personalizado en el contenedor de servicios.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La colección de servicios con el manejador registrado.</returns>
        public static IServiceCollection AddCustomException(this IServiceCollection services)
        {
            services.AddExceptionHandler<ExceptionHandler>();
            return services;
        }

        /// <summary>
        /// Configura el middleware de manejo de excepciones en el pipeline de la aplicación.
        /// </summary>
        /// <param name="app">Builder de la aplicación.</param>
        /// <returns>El builder de la aplicación configurado.</returns>
        public static IApplicationBuilder UseCustomException(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(x => { });

            return app;
        }
    }
}
