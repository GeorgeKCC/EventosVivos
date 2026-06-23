using Microsoft.EntityFrameworkCore;
using Transversal.Database.Entities;

namespace Transversal.Database
{
    public class EventosVivosDbContext(DbContextOptions<EventosVivosDbContext> dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<TipoEvento> TipoEventos { get; set; }
        public DbSet<EstadoReserva> EstadoReserva { get; set; }
        public DbSet<EstadoEvento> EstadoEvento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
