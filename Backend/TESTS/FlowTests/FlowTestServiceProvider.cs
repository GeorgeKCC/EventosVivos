using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModuloEvento;
using ModuloReserva;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace FlowTests
{
    /// <summary>
    /// Configura el ServiceProvider con EF Core InMemory y todos los servicios
    /// de los módulos Evento y Reserva para ejecutar tests de integración de flujo.
    /// </summary>
    public static class FlowTestServiceProvider
    {
        public static ServiceProvider Create(string databaseName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<EventosVivosDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            services.ModuloEventoRegisterServices();
            services.ModuloReservaRegisterServices();

            var provider = services.BuildServiceProvider();

            SeedData(provider);

            return provider;
        }

        private static void SeedData(ServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            context.Venues.Add(new Venue
            {
                Id = 1,
                Nombre = "Arena Central",
                Capacidad = 500,
                Ciudad = "Bogotá"
            });

            context.TipoEventos.Add(new TipoEvento
            {
                Id = 1,
                Nombre = "Concierto"
            });

            context.EstadoEvento.AddRange(
                new EstadoEvento { Id = (int)EstadoEnum.Activo, Nombre = "Activo" },
                new EstadoEvento { Id = (int)EstadoEnum.Completado, Nombre = "Completado" }
            );

            context.EstadoReserva.AddRange(
                new EstadoReserva { Id = (int)EstadoReservaEnum.PendientePago, Nombre = "Pendiente Pago" },
                new EstadoReserva { Id = (int)EstadoReservaEnum.Confirmada, Nombre = "Confirmada" },
                new EstadoReserva { Id = (int)EstadoReservaEnum.Cancelada, Nombre = "Cancelada" }
            );

            context.SaveChanges();
        }
    }
}
