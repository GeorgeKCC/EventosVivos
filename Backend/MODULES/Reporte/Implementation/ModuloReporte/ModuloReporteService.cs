using Microsoft.Extensions.DependencyInjection;
using ModuloReporte.ImplementationUseCase;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporte.Strategies.StrategyImplementation;
using ModuloReporteContract;

namespace ModuloReporte
{
    public static class ModuloReporteService
    {
        /// <summary>
        /// Registra los servicios del módulo de reportes en el contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La colección de servicios con los servicios del módulo registrados.</returns>
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
