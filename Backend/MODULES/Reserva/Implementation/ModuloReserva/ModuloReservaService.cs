using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModuloReserva.ImplementationUseCase;
using ModuloReserva.Strategies.StrategyContract;
using ModuloReserva.Strategies.StrategyImplementation;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;

namespace ModuloReserva
{
    public static class ModuloReservaService
    {
        /// <summary>
        /// Registra los servicios del módulo de reservas en el contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La colección de servicios con los servicios del módulo registrados.</returns>
        public static IServiceCollection ModuloReservaRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IObtenerTodoReservas, ObtenerTodoReservas>();
            services.AddScoped<IConfirmarReservaUseCase, ConfirmarReservaUseCase>();
            services.AddScoped<ICrearReservaUserCase, CrearReservaUserCase>();
            services.AddScoped<IStrategyReserva, StrategyReservaConfirmado>();
            services.AddScoped<IStrategyReserva, StrategyReservaCancelada>();

            services.AddScoped<IValidator<RequestCrearReserva>, ValidationCrearReservaUserCase>();
            services.AddScoped<IValidator<RequestEstadoReserva>, ValidationConfirmarReservaUseCase>();
            return services;
        }
    }
}
