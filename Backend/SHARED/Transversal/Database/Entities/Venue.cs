using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("Venues")]
    [Index(nameof(Id), IsUnique = true)]
    [Index(nameof(Nombre), IsUnique = true)]
    public class Venue
    {
        [Key]
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required int Capacidad { get; set; }
        public required string Ciudad { get; set; }

    }
}
