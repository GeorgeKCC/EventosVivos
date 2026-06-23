using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;

namespace ModuloEvento
{
    public static class ModuloEventoService
    {
        public static IServiceCollection ModuloEventoRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICrearEventoUseCase, CrearEventoUseCase>();
            services.AddScoped<IBuscarEventoUseCase, BuscarEventoUseCase>();

            services.AddScoped<IValidator<RequestCrearEvento>, ValidationCrearEventoUseCase>();
            return services;
        }
    }
}
