using Microsoft.Extensions.DependencyInjection;
using ModuloTarea.Jobs;
using Quartz;

namespace ModuloTarea
{
    public static class ModuloTareaService
    {
        /// <summary>
        /// Registra Quartz como Hosted Service y configura el job CompletarEventosJob
        /// con una expresión cron programada.
        /// </summary>
        /// <param name="services">Service collection del host</param>
        /// <param name="cronExpression">
        /// Expresión cron de Quartz. Por defecto: cada 5 minutos ("0 0/5 * * * ?").
        /// Ejemplos:
        ///   - Cada minuto: "0 0/1 * * * ?"
        ///   - Cada hora: "0 0 0/1 * * ?"
        ///   - Todos los días a medianoche: "0 0 0 * * ?"
        /// </param>
        public static IServiceCollection ModuloTareaRegisterServices(this IServiceCollection services,
            string cronExpression = "0 0/5 * * * ?")
        {
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("CompletarEventosJob", "EventosGroup");

                q.AddJob<CompletarEventosJob>(opts => opts
                    .WithIdentity(jobKey)
                    .WithDescription("Verifica eventos cuya fecha/hora de fin ya pasó y los marca como Completados"));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("CompletarEventosTrigger", "EventosGroup")
                    .WithDescription("Trigger cron para verificar eventos finalizados")
                    .WithCronSchedule(cronExpression));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
