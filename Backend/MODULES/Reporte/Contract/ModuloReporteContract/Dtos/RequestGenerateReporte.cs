using ModuloReporteContract.Enums;

namespace ModuloReporteContract.Dtos
{
    public record RequestGenerateReporte(int EventoId, TipoReporte TipoReporte);
}
