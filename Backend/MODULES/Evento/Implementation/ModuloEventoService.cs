using Microsoft.Extensions.DependencyInjection;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract;

namespace ModuloEvento
{
    public static class ModuloEventoService
    {
        public static IServiceCollection ModuloEventoRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICrearEventoUseCase, CrearEventoUseCase>();
            return services;
        }
    }
}
