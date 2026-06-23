using Microsoft.Extensions.DependencyInjection;
using ModuloReporte.ImplementationUseCase;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract;

namespace ModuloReporte
{
    public static class ModuloReporteService
    {
        public static IServiceCollection ModuloReporteRegisterService(this IServiceCollection services)
        {
            services.AddScoped<IStrategyReporte, EstadoDelEventoStrategy>();
            services.AddScoped<IStrategyReporte, PorcentajeDeOcupaciónStrategy>();
            services.AddScoped<IStrategyReporte, TotalEntradasDisponiblesStrategy>();
            services.AddScoped<IStrategyReporte, TotalEntradasVendidasStrategy>();
            services.AddScoped<IStrategyReporte, TotalIngresosStrategy>();
            services.AddScoped<IGenerarReporteUseCase, GenerarReporteUseCase>();

            return services;
        }
    }
}
