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
    internal class TotalEntradasVendidasStrategy(EventosVivosDbContext eventosVivosDbContext) : IStrategyReporte
    {
        public int TipoReporteId => (int)TipoReporte.TotalEntradas;

        /// <summary>
        /// Genera un reporte Excel con el total de entradas vendidas y detalle por comprador.
        /// </summary>
        /// <param name="EventoId">Identificador del evento.</param>
        /// <returns>Resultado del reporte con el archivo Excel generado.</returns>
        public async Task<ReporteResult> ExecuteAsync(int EventoId)
        {
            var evento = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{EventoId}");

            var reservasConfirmadas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId && r.EstadoReservaId == (int)EstadoReservaEnum.Confirmada)
                .Select(r => new
                {
                    r.Id,
                    r.NombreComprador,
                    r.EmailComprador,
                    r.Cantidad
                })
                .ToListAsync();

            var totalEntradas = reservasConfirmadas.Sum(r => r.Cantidad);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Entradas Vendidas");

            worksheet.Cell(1, 1).Value = "Evento";
            worksheet.Cell(1, 2).Value = evento.Titulo;
            worksheet.Cell(2, 1).Value = "Venue";
            worksheet.Cell(2, 2).Value = evento.Venue.Nombre;
            worksheet.Cell(3, 1).Value = "Fecha Inicio";
            worksheet.Cell(3, 2).Value = evento.InicioEvento.ToString();
            worksheet.Cell(4, 1).Value = "Total Entradas Vendidas";
            worksheet.Cell(4, 2).Value = totalEntradas;
            worksheet.Cell(5, 1).Value = "Capacidad Máxima";
            worksheet.Cell(5, 2).Value = evento.CapacidadMaxima;

            var headerRow = 7;
            worksheet.Cell(headerRow, 1).Value = "Reserva Id";
            worksheet.Cell(headerRow, 2).Value = "Comprador";
            worksheet.Cell(headerRow, 3).Value = "Email";
            worksheet.Cell(headerRow, 4).Value = "Cantidad";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < reservasConfirmadas.Count; i++)
            {
                var row = headerRow + 1 + i;
                worksheet.Cell(row, 1).Value = reservasConfirmadas[i].Id;
                worksheet.Cell(row, 2).Value = reservasConfirmadas[i].NombreComprador;
                worksheet.Cell(row, 3).Value = reservasConfirmadas[i].EmailComprador;
                worksheet.Cell(row, 4).Value = reservasConfirmadas[i].Cantidad;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReporteResult(
                stream.ToArray(),
                $"EntradasVendidas_Evento_{EventoId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
