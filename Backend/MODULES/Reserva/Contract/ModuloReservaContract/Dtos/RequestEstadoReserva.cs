using Transversal.Database.Enums;

namespace ModuloReservaContract.Dtos
{
    public record RequestEstadoReserva(EstadoReservaEnum EstadoReservaEnum, int ReservaId);
}
