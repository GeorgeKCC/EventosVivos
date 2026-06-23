namespace ModuloEventoContract.Dtos
{
    public record class RequestCrearEvento(
        string Titulo,
        string Descripcion,
        int CapacidadMaxima,
        DateOnly InicioEvento,
        DateTime IniciaHora,
        DateOnly FinEvento,
        DateTime FinHora,
        Decimal Precio,
        int VenueId,
        int TipoEventoId
    );
}
