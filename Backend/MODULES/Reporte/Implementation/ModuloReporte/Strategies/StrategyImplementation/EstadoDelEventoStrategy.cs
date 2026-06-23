using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract.Dtos;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporte.Strategies.StrategyImplementation
{
    internal class EstadoDelEventoStrategy(EventosVivosDbContext eventosVivosDbContext) : IStrategyReporte
    {
        public int TipoReporteId => (int)TipoReporte.EstadoDelEvento;

        /// <summary>
        /// Genera un reporte Excel con el estado completo de un evento.
        /// </summary>
        /// <param name="EventoId">Identificador del evento.</param>
        /// <returns>Resultado del reporte con el archivo Excel generado.</returns>
        public async Task<ReporteResult> ExecuteAsync(int EventoId)
        {
            var evento = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.TipoEvento)
                .Include(e => e.EstadoEvento)
                .FirstOrDefaultAsync(e => e.Id == EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{EventoId}");

            var totalReservadas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId && !r.EsPerdida)
                .SumAsync(r => r.Cantidad);

            var entradasDisponibles = evento.CapacidadMaxima - totalReservadas;
            var porcentajeOcupacion = evento.CapacidadMaxima > 0
                ? Math.Round((double)totalReservadas / evento.CapacidadMaxima * 100, 2)
                : 0;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estado del Evento");

            worksheet.Cell(1, 1).Value = "Campo";
            worksheet.Cell(1, 2).Value = "Detalle";

            var headerRange = worksheet.Range(1, 1, 1, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            worksheet.Cell(2, 1).Value = "Evento";
            worksheet.Cell(2, 2).Value = evento.Titulo;
            worksheet.Cell(3, 1).Value = "Descripción";
            worksheet.Cell(3, 2).Value = evento.Descripcion;
            worksheet.Cell(4, 1).Value = "Estado";
            worksheet.Cell(4, 2).Value = evento.EstadoEvento.Nombre;
            worksheet.Cell(5, 1).Value = "Tipo Evento";
            worksheet.Cell(5, 2).Value = evento.TipoEvento.Nombre;
            worksheet.Cell(6, 1).Value = "Venue";
            worksheet.Cell(6, 2).Value = evento.Venue.Nombre;
            worksheet.Cell(7, 1).Value = "Ciudad";
            worksheet.Cell(7, 2).Value = evento.Venue.Ciudad;
            worksheet.Cell(8, 1).Value = "Fecha Inicio";
            worksheet.Cell(8, 2).Value = evento.InicioEvento.ToString();
            worksheet.Cell(9, 1).Value = "Hora Inicio";
            worksheet.Cell(9, 2).Value = evento.IniciaHora.ToString();
            worksheet.Cell(10, 1).Value = "Fecha Fin";
            worksheet.Cell(10, 2).Value = evento.FinEvento.ToString();
            worksheet.Cell(11, 1).Value = "Hora Fin";
            worksheet.Cell(11, 2).Value = evento.FinHora.ToString();
            worksheet.Cell(12, 1).Value = "Precio";
            worksheet.Cell(12, 2).Value = evento.Precio;
            worksheet.Cell(12, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(13, 1).Value = "Capacidad Máxima";
            worksheet.Cell(13, 2).Value = evento.CapacidadMaxima;
            worksheet.Cell(14, 1).Value = "Total Reservadas";
            worksheet.Cell(14, 2).Value = totalReservadas;
            worksheet.Cell(15, 1).Value = "Entradas Disponibles";
            worksheet.Cell(15, 2).Value = entradasDisponibles;
            worksheet.Cell(16, 1).Value = "Porcentaje de Ocupación";
            worksheet.Cell(16, 2).Value = $"{porcentajeOcupacion}%";

            var estadoCell = worksheet.Cell(4, 2);
            estadoCell.Style.Font.Bold = true;
            if (evento.EstadoEvento.Nombre.Equals("Activo", StringComparison.OrdinalIgnoreCase))
                estadoCell.Style.Font.FontColor = XLColor.Green;
            else if (evento.EstadoEvento.Nombre.Equals("Completado", StringComparison.OrdinalIgnoreCase))
                estadoCell.Style.Font.FontColor = XLColor.Blue;
            else
                estadoCell.Style.Font.FontColor = XLColor.Red;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReporteResult(
                stream.ToArray(),
                $"EstadoEvento_Evento_{EventoId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
