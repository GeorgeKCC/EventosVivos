import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ResponseReserva } from '../models';

@Injectable({ providedIn: 'root' })
export class BuscarReservaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Reservas`;

  getAll(): Observable<ResponseReserva[]> {
    return this.http.get<ResponseReserva[]>(this.baseUrl);
  }
}
