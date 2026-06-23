using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;

namespace ModuloEvento
{
    public static class ModuloEventoService
    {
        /// <summary>
        /// Registra los servicios del módulo de eventos en el contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La colección de servicios con los servicios del módulo registrados.</returns>
        public static IServiceCollection ModuloEventoRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICrearEventoUseCase, CrearEventoUseCase>();
            services.AddScoped<IBuscarEventoUseCase, BuscarEventoUseCase>();

            services.AddScoped<IValidator<RequestCrearEvento>, ValidationCrearEventoUseCase>();
            return services;
        }
    }
}
