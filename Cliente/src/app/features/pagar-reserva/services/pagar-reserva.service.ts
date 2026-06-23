import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestEstadoReserva } from '../models';

@Injectable({ providedIn: 'root' })
export class PagarReservaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Reservas`;

  payment(request: RequestEstadoReserva): Observable<string> {
    return this.http.post(`${this.baseUrl}/Payment`, request, { responseType: 'text' });
  }
}
