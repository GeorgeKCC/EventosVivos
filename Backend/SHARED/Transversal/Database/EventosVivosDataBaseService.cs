using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transversal.Database.Entities;
using System.Security.Cryptography;

namespace Transversal.Database
{
    public static class EventosVivosDataBaseService
    {
        public static IServiceCollection EventosVivosDataBaseRegisterService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection") ??
                                   throw new Exception("Not found configuration connection string sql");

            services.AddDbContext<EventosVivosDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceProvider EventosVivosDataBaseExecuteSeed(this IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            context.Database.MigrateAsync().Wait();

            if (!context.TipoEventos.Any())
            {
                context.TipoEventos.AddRange(
                    new TipoEvento { Nombre = "Conferencia" },
                    new TipoEvento { Nombre = "Taller" },
                    new TipoEvento { Nombre = "Concierto" }
                );
                context.SaveChanges();
            }

            if (!context.Venues.Any())
            {
                context.Venues.AddRange(
                    new Venue { Nombre = "Auditorio Central", Capacidad = 200, Ciudad = "Bogotá" },
                    new Venue { Nombre = "Sala Norte", Capacidad = 50, Ciudad = "Bogotá" },
                    new Venue { Nombre = "Arena Sur", Capacidad = 500, Ciudad = "Medellín" }
                );
                context.SaveChanges();
            }

            if (!context.EstadoReserva.Any())
            {
                context.EstadoReserva.AddRange(
                    new EstadoReserva { Nombre = "Pendiente Pago" },
                    new EstadoReserva { Nombre = "Confirmada" },
                    new EstadoReserva { Nombre = "Cancelada" }
                );
                context.SaveChanges();
            }

            if (!context.EstadoEvento.Any())
            {
                context.EstadoEvento.AddRange(
                    new EstadoEvento { Nombre = "Activo" },
                    new EstadoEvento { Nombre = "Completado" }
                );
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                context.Roles.Add(new Rol { Nombre = "Admin" });
                context.SaveChanges();
            }

            if (!context.Usuarios.Any())
            {
                var rolAdmin = context.Roles.First(r => r.Nombre == "Admin");
                var passwordHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("admin")));
                context.Usuarios.Add(new Usuario
                {
                    Username = "admin",
                    PasswordHash = passwordHash,
                    RolId = rolAdmin.Id
                });
                context.SaveChanges();
            }

            return serviceProvider;
        }
    }
}
