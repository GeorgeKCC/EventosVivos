namespace ModuloReporteContract.Dtos
{
    public record ReporteResult(byte[] FileContent, string FileName, string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}
