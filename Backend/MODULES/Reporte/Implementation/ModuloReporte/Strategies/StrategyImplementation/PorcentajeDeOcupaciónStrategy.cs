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
    internal class PorcentajeDeOcupaciónStrategy(EventosVivosDbContext eventosVivosDbContext) : IStrategyReporte
    {
        public int TipoReporteId => (int)TipoReporte.PorcentajeDeOcupacion;

        public async Task<ReporteResult> ExecuteAsync(int EventoId)
        {
            var evento = await eventosVivosDbContext.Eventos
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.EstadoEvento)
                .FirstOrDefaultAsync(e => e.Id == EventoId)
                ?? throw new NotFoundCustomException($"No se encontró evento, id:{EventoId}");

            var totalConfirmadas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId
                         && r.EstadoReservaId == (int)EstadoReservaEnum.Confirmada
                         && !r.EsPerdida)
                .SumAsync(r => r.Cantidad);

            var totalPendientes = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId
                         && r.EstadoReservaId == (int)EstadoReservaEnum.PendientePago
                         && !r.EsPerdida)
                .SumAsync(r => r.Cantidad);

            var totalPerdidas = await eventosVivosDbContext.Reservas
                .AsNoTracking()
                .Where(r => r.EventoId == EventoId && r.EsPerdida)
                .SumAsync(r => r.Cantidad);

            var totalOcupadas = totalConfirmadas + totalPendientes + totalPerdidas;
            var entradasDisponibles = evento.CapacidadMaxima - totalOcupadas;
            var porcentajeOcupacion = evento.CapacidadMaxima > 0
                ? Math.Round((double)totalOcupadas / evento.CapacidadMaxima * 100, 2)
                : 0;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Porcentaje Ocupación");

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

            var headerRow = 7;
            worksheet.Cell(headerRow, 1).Value = "Métrica";
            worksheet.Cell(headerRow, 2).Value = "Valor";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            worksheet.Cell(headerRow + 1, 1).Value = "Capacidad Máxima";
            worksheet.Cell(headerRow + 1, 2).Value = evento.CapacidadMaxima;
            worksheet.Cell(headerRow + 2, 1).Value = "Entradas Confirmadas";
            worksheet.Cell(headerRow + 2, 2).Value = totalConfirmadas;
            worksheet.Cell(headerRow + 3, 1).Value = "Entradas Pendientes de Pago";
            worksheet.Cell(headerRow + 3, 2).Value = totalPendientes;
            worksheet.Cell(headerRow + 4, 1).Value = "Entradas Perdidas (penalización)";
            worksheet.Cell(headerRow + 4, 2).Value = totalPerdidas;
            worksheet.Cell(headerRow + 5, 1).Value = "Total Ocupadas";
            worksheet.Cell(headerRow + 5, 2).Value = totalOcupadas;
            worksheet.Cell(headerRow + 6, 1).Value = "Entradas Disponibles";
            worksheet.Cell(headerRow + 6, 2).Value = entradasDisponibles;
            worksheet.Cell(headerRow + 7, 1).Value = "Porcentaje de Ocupación";
            worksheet.Cell(headerRow + 7, 2).Value = $"{porcentajeOcupacion}%";

            var porcentajeCell = worksheet.Cell(headerRow + 7, 2);
            porcentajeCell.Style.Font.Bold = true;
            if (porcentajeOcupacion >= 90)
                porcentajeCell.Style.Font.FontColor = XLColor.Red;
            else if (porcentajeOcupacion >= 70)
                porcentajeCell.Style.Font.FontColor = XLColor.Orange;
            else
                porcentajeCell.Style.Font.FontColor = XLColor.Green;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ReporteResult(
                stream.ToArray(),
                $"PorcentajeOcupacion_Evento_{EventoId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
