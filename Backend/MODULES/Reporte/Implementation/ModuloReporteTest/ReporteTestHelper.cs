using Microsoft.EntityFrameworkCore;
using Transversal.Database;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloReporteTest
{
    internal static class ReporteTestHelper
    {
        public static EventosVivosDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new EventosVivosDbContext(options);
        }

        public static void SeedData(EventosVivosDbContext context)
        {
            context.Venues.Add(new Venue
            {
                Id = 1,
                Nombre = "Arena CDMX",
                Capacidad = 200,
                Ciudad = "Ciudad de México"
            });

            context.TipoEventos.Add(new TipoEvento { Id = 1, Nombre = "Concierto" });

            context.EstadoEvento.AddRange(
                new EstadoEvento { Id = 1, Nombre = "Activo" },
                new EstadoEvento { Id = 2, Nombre = "Completado" }
            );

            context.EstadoReserva.AddRange(
                new EstadoReserva { Id = 1, Nombre = "PendientePago" },
                new EstadoReserva { Id = 2, Nombre = "Confirmada" },
                new EstadoReserva { Id = 3, Nombre = "Cancelada" }
            );

            context.Eventos.Add(new Evento
            {
                Id = 1,
                Titulo = "Concierto Rock",
                Descripcion = "Gran concierto de rock en vivo",
                CapacidadMaxima = 100,
                InicioEvento = new DateOnly(2025, 8, 15),
                IniciaHora = new TimeOnly(20, 0),
                FinEvento = new DateOnly(2025, 8, 15),
                FinHora = new TimeOnly(23, 0),
                Precio = 150m,
                VenueId = 1,
                TipoEventoId = 1,
                EstadoId = 1
            });

            context.Reservas.AddRange(
                new Reserva
                {
                    Id = 1,
                    Cantidad = 5,
                    NombreComprador = "Juan",
                    EmailComprador = "juan@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Confirmada
                },
                new Reserva
                {
                    Id = 2,
                    Cantidad = 3,
                    NombreComprador = "Maria",
                    EmailComprador = "maria@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Confirmada
                },
                new Reserva
                {
                    Id = 3,
                    Cantidad = 2,
                    NombreComprador = "Pedro",
                    EmailComprador = "pedro@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.PendientePago
                },
                new Reserva
                {
                    Id = 4,
                    Cantidad = 4,
                    NombreComprador = "Ana",
                    EmailComprador = "ana@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Cancelada,
                    EsPerdida = true
                },
                new Reserva
                {
                    Id = 5,
                    Cantidad = 0,
                    NombreComprador = "Luis",
                    EmailComprador = "luis@mail.com",
                    EventoId = 1,
                    EstadoReservaId = (int)EstadoReservaEnum.Cancelada,
                    EsPerdida = false,
                    FechaCancelacion = DateTime.Now
                }
            );

            context.SaveChanges();
        }
    }
}
