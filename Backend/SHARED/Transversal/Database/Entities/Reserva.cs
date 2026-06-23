using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transversal.Database.Entities
{
    [Table("Reserva")]
    [Index(nameof(Id), IsUnique = true)]
    [Index(nameof(NombreComprador), IsUnique = true)]
    [Index(nameof(EmailComprador), IsUnique = true)]
    public class Reserva
    {
        [Key]
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public required string NombreComprador { get; set; }
        public required string EmailComprador { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public bool EsPerdida { get; set; }

        [ForeignKey("Evento")]
        public required int EventoId { get; set; }
        public virtual Evento Evento { get; set; } = null!;

        [ForeignKey("EstadoReserva")]
        public required int EstadoReservaId { get; set; }
        public virtual EstadoReserva EstadoReserva { get; set; } = null!;
    }
}
