using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ModuloReporte.Strategies.StrategyContract;
using ModuloReporteContract.Dtos;
using ModuloReporteContract.Enums;
using Transversal.Database;
using Transversal.Exceptions.Customs;

namespace ModuloReporte.Strategies.StrategyImplementation
{
    internal class TotalEntradasDisponiblesStrategy(EventosVivosDbContext eventosVivosDbContext) : IStrategyReporte
    {
        public int TipoReporteId => (int)TipoReporte.TotalEntradasDisponibles;

        /// <summary>
        /// Genera un reporte Excel con las entradas disponibles y desglose por estado.
        /// </summary>
        /// <param name="EventoId">Identificador del evento.</param>
        /// <returns>Resultado del reporte con el archivo Excel generado.</returns>
        public async Task<ReporteResult> ExecuteAsync(int EventoId)
        {
            var evento = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.EstadoEvento)
                .FirstOrDefaultAsync(e => e.Id == EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{EventoId}");

            var totalReservadas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId && !r.EsPerdida)
                .SumAsync(r => r.Cantidad);

            var entradasDisponibles = evento.CapacidadMaxima - totalReservadas;

            var reservasPorEstado = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Include(r => r.EstadoReserva)
                .Where(r => r.EventoId == EventoId && !r.EsPerdida)
                .GroupBy(r => r.EstadoReserva.Nombre)
                .Select(g => new
                {
                    Estado = g.Key,
                    Total = g.Sum(r => r.Cantidad)
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Entradas Disponibles");

            worksheet.Cell(1, 1).Value = "Evento";
            worksheet.Cell(1, 2).Value = evento.Titulo;
            worksheet.Cell(2, 1).Value = "Venue";
            worksheet.Cell(2, 2).Value = evento.Venue.Nombre;
            worksheet.Cell(3, 1).Value = "Estado Evento";
            worksheet.Cell(3, 2).Value = evento.EstadoEvento.Nombre;
            worksheet.Cell(4, 1).Value = "Fecha Inicio";
            worksheet.Cell(4, 2).Value = evento.InicioEvento.ToString();
            worksheet.Cell(5, 1).Value = "Hora Inicio";
            worksheet.Cell(5, 2).Value = evento.IniciaHora.ToString();
            worksheet.Cell(6, 1).Value = "Capacidad Máxima";
            worksheet.Cell(6, 2).Value = evento.CapacidadMaxima;
            worksheet.Cell(7, 1).Value = "Total Reservadas";
            worksheet.Cell(7, 2).Value = totalReservadas;
            worksheet.Cell(8, 1).Value = "Entradas Disponibles";
            worksheet.Cell(8, 2).Value = entradasDisponibles;

            var headerRow = 10;
            worksheet.Cell(headerRow, 1).Value = "Estado Reserva";
            worksheet.Cell(headerRow, 2).Value = "Cantidad";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < reservasPorEstado.Count; i++)
            {
                var row = headerRow + 1 + i;
                worksheet.Cell(row, 1).Value = reservasPorEstado[i].Estado;
                worksheet.Cell(row, 2).Value = reservasPorEstado[i].Total;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReporteResult(
                stream.ToArray(),
                $"EntradasDisponibles_Evento_{EventoId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
