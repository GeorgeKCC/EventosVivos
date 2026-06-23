using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("Events")]
    [Index(nameof(Id), IsUnique = true)]
    [Index(nameof(Titulo), IsUnique = true)]
    public class Evento
    {
        [Key]
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Descripcion { get; set; }
        public required int CapacidadMaxima { get; set; }
        public required DateOnly InicioEvento { get; set; }
        public required TimeOnly IniciaHora { get; set; }
        public required DateOnly FinEvento { get; set; }
        public required TimeOnly FinHora { get; set; }
        public required Decimal Precio { get; set; }

        [ForeignKey(nameof(Venue))]
        public required int VenueId { get; set; }
        public virtual Venue Venue { get; set; } = null!;

        [ForeignKey(nameof(TipoEvento))]
        public required int TipoEventoId { get; set; }
        public virtual TipoEvento TipoEvento { get; set; } = null!;

        [ForeignKey(nameof(EstadoEvento))]
        public required int EstadoId { get; set; }
        public virtual EstadoEvento EstadoEvento { get; set; } = null!;
    }
}
