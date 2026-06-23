using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Transversal.Exceptions.Handler;

namespace Transversal.Exceptions
{
    public static class ExceptionRegisterServices
    {
        public static IServiceCollection AddCustomException(this IServiceCollection services)
        {
            services.AddExceptionHandler<ExceptionHandler>();
            return services;
        }

        public static IApplicationBuilder UseCustomException(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(x => { });

            return app;
        }
    }
}
