import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReporteService } from './services';
import { RequestGenerateReporte, TipoReporte } from './models';

@Component({
  selector: 'app-reportes',
  imports: [FormsModule],
  templateUrl: './reportes.html',
  styleUrl: './reportes.scss'
})
export class Reportes {
  private readonly reporteService = inject(ReporteService);
  readonly router = inject(Router);

  form: RequestGenerateReporte = {
    eventoId: 0,
    tipoReporte: TipoReporte.TotalEntradas
  };

  tiposReporte = [
    { value: TipoReporte.TotalEntradas, label: 'Total Entradas' },
    { value: TipoReporte.TotalEntradasDisponibles, label: 'Total Entradas Disponibles' },
    { value: TipoReporte.PorcentajeDeOcupacion, label: 'Porcentaje de Ocupación' },
    { value: TipoReporte.TotalDeIngresos, label: 'Total de Ingresos' },
    { value: TipoReporte.EstadoDelEvento, label: 'Estado del Evento' }
  ];

  loading = signal(false);
  error = signal('');

  download(): void {
    this.loading.set(true);
    this.error.set('');
    this.reporteService.downloadReport(this.form).subscribe({
      next: (blob) => {
        this.loading.set(false);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `reporte_evento_${this.form.eventoId}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set('Error al descargar el reporte');
      }
    });
  }
}
