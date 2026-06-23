import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestCrearReserva } from '../models';

@Injectable({ providedIn: 'root' })
export class ReservaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Reservas`;

  create(request: RequestCrearReserva): Observable<void> {
    return this.http.post<void>(this.baseUrl, request);
  }
}
