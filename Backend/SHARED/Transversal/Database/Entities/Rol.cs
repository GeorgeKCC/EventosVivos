using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("Rol")]
    public class Rol
    {
        [Key]
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = [];
    }
}
