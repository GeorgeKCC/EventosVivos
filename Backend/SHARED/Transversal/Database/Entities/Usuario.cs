using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public int RolId { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol Rol { get; set; } = null!;
    }
}
