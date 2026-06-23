import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestGenerateReporte } from '../models';

@Injectable({ providedIn: 'root' })
export class ReporteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Reporte`;

  downloadReport(request: RequestGenerateReporte): Observable<Blob> {
    return this.http.get(this.baseUrl, {
      params: {
        eventoId: request.eventoId,
        tipoReporte: request.tipoReporte
      },
      responseType: 'blob'
    });
  }
}
