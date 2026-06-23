import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestCrearEvento, Venue, TipoEvento } from '../models';

@Injectable({ providedIn: 'root' })
export class CrearEventoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Evento`;

  create(request: RequestCrearEvento): Observable<void> {
    return this.http.post<void>(this.baseUrl, request);
  }

  getAllVenue(): Observable<Venue[]> {
    return this.http.get<Venue[]>(`${this.baseUrl}/GetAllVenue`);
  }

  getAllTipoEvento(): Observable<TipoEvento[]> {
    return this.http.get<TipoEvento[]>(`${this.baseUrl}/GetAllTipoEvento`);
  }
}
