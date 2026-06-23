import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestBuscarEvento, ResponseBuscarEvento } from '../models';

@Injectable({ providedIn: 'root' })
export class EventoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Evento`;

  search(filters: RequestBuscarEvento): Observable<ResponseBuscarEvento[]> {
    let params = new HttpParams();

    if (filters.titulo) params = params.set('titulo', filters.titulo);
    if (filters.tipoEventoId) params = params.set('tipoEventoId', filters.tipoEventoId.toString());
    if (filters.venueId) params = params.set('venueId', filters.venueId.toString());
    if (filters.estadoId) params = params.set('estadoId', filters.estadoId.toString());
    if (filters.fechaInicioEventoRange) params = params.set('fechaInicioEventoRange', filters.fechaInicioEventoRange);
    if (filters.fechaFinEventoRange) params = params.set('fechaFinEventoRange', filters.fechaFinEventoRange);

    return this.http.get<ResponseBuscarEvento[]>(this.baseUrl, { params });
  }
}
