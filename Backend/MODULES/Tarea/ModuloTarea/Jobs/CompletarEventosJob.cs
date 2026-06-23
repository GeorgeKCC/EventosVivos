using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Transversal.Database;
using Transversal.Database.Enums;

namespace ModuloTarea.Jobs
{
    [DisallowConcurrentExecution]
    public class CompletarEventosJob(EventosVivosDbContext dbContext,
                                     ILogger<CompletarEventosJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var ahora = DateTime.Now;

            logger.LogInformation("CompletarEventosJob: Iniciando verificación de eventos finalizados - {Time}", ahora);

            var eventosFinalizados = await dbContext.Eventos
                .Where(e => e.EstadoId == (int)EstadoEnum.Activo)
                .ToListAsync(context.CancellationToken);

            var eventosACompletar = eventosFinalizados
                .Where(e => e.FinEvento.ToDateTime(e.FinHora) <= ahora)
                .ToList();

            if (eventosACompletar.Count == 0)
            {
                logger.LogInformation("CompletarEventosJob: No hay eventos pendientes de completar");
                return;
            }

            foreach (var evento in eventosACompletar)
            {
                evento.EstadoId = (int)EstadoEnum.Completado;
                logger.LogInformation("CompletarEventosJob: Evento '{Titulo}' (Id: {Id}) marcado como completado",
                    evento.Titulo, evento.Id);
            }

            await dbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("CompletarEventosJob: Se completaron {Count} evento(s)", eventosACompletar.Count);
        }
    }
}
