namespace ModuloEventoContract.Dtos
{
    public record ResponseBuscarEvento(
        string Titulo, 
        string Descripción,
        int VenuedId,
        string VenueName,
        int CapacidadMaxima,
        DateOnly FechaInicioEvento,
        TimeOnly HoraInicioEvento,
        DateOnly FechaFinEvento,
        TimeOnly HoraFinEvento,
        int TipoEventoId,
        string TipoEventoNombre,
        int EstadoId,
        string EstadoNombre);

}
