namespace ModuloEventoContract.Dtos
{
    public record RequestBuscarEvento(
        int? TipoEventoId, 
        DateOnly? FechaInicioEventoRange, 
        DateOnly? FechaFinEventoRange, 
        int? VenueId,
        int? EstadoId,
        string? Titulo);
}
