namespace ModuloEventoContract.Dtos
{
    public record class RequestCrearEvento(
        string Titulo,
        string Descripcion,
        int CapacidadMaxima,
        DateOnly InicioEvento,
        TimeOnly IniciaHora,
        DateOnly FinEvento,
        TimeOnly FinHora,
        Decimal Precio,
        int VenueId,
        int TipoEventoId
    );
}
