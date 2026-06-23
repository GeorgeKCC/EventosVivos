using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract.Dtos;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Database.Enums;
using Transversal.Exceptions.Customs;

namespace ModuloReporte.Strategies.StrategyImplementation
{
    internal class TotalIngresosStrategy(EventosVivosDbContext eventosVivosDbContext) : IStrategyReporte
    {
        public int TipoReporteId => (int)TipoReporte.TotalDeIngresos;

        public async Task<ReporteResult> ExecuteAsync(int EventoId)
        {
            var evento = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.EstadoEvento)
                .FirstOrDefaultAsync(e => e.Id == EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{EventoId}");

            var reservasConfirmadas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId
                         && r.EstadoReservaId == (int)EstadoReservaEnum.Confirmada
                         && !r.EsPerdida)
                .Select(r => new
                {
                    r.Id,
                    r.NombreComprador,
                    r.EmailComprador,
                    r.Cantidad,
                    Subtotal = r.Cantidad * evento.Precio
                })
                .ToListAsync();

            var totalEntradas = reservasConfirmadas.Sum(r => r.Cantidad);
            var totalIngresos = totalEntradas * evento.Precio;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Total Ingresos");

            worksheet.Cell(1, 1).Value = "Evento";
            worksheet.Cell(1, 2).Value = evento.Titulo;
            worksheet.Cell(2, 1).Value = "Venue";
            worksheet.Cell(2, 2).Value = evento.Venue.Nombre;
            worksheet.Cell(3, 1).Value = "Estado Evento";
            worksheet.Cell(3, 2).Value = evento.EstadoEvento.Nombre;
            worksheet.Cell(4, 1).Value = "Fecha Inicio";
            worksheet.Cell(4, 2).Value = evento.InicioEvento.ToString();
            worksheet.Cell(5, 1).Value = "Precio por Entrada";
            worksheet.Cell(5, 2).Value = evento.Precio;
            worksheet.Cell(5, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(6, 1).Value = "Total Entradas Confirmadas";
            worksheet.Cell(6, 2).Value = totalEntradas;
            worksheet.Cell(7, 1).Value = "Total Ingresos";
            worksheet.Cell(7, 2).Value = totalIngresos;
            worksheet.Cell(7, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(7, 2).Style.Font.Bold = true;

            var headerRow = 9;
            worksheet.Cell(headerRow, 1).Value = "Reserva Id";
            worksheet.Cell(headerRow, 2).Value = "Comprador";
            worksheet.Cell(headerRow, 3).Value = "Email";
            worksheet.Cell(headerRow, 4).Value = "Cantidad";
            worksheet.Cell(headerRow, 5).Value = "Subtotal";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < reservasConfirmadas.Count; i++)
            {
                var row = headerRow + 1 + i;
                worksheet.Cell(row, 1).Value = reservasConfirmadas[i].Id;
                worksheet.Cell(row, 2).Value = reservasConfirmadas[i].NombreComprador;
                worksheet.Cell(row, 3).Value = reservasConfirmadas[i].EmailComprador;
                worksheet.Cell(row, 4).Value = reservasConfirmadas[i].Cantidad;
                worksheet.Cell(row, 5).Value = reservasConfirmadas[i].Subtotal;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReporteResult(
                stream.ToArray(),
                $"TotalIngresos_Evento_{EventoId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
