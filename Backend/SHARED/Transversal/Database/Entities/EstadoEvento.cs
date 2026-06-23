using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("EstadoEvento")]
    [Index(nameof(Id), IsUnique = true)]
    [Index(nameof(Nombre), IsUnique = true)]
    public class EstadoEvento
    {
        [Key]
        public int Id { get; set; }
        public required string Nombre { get; set; }
    }
}
