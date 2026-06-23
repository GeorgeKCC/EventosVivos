import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, finalize } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RequestLogin, ResponseLogin } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/Auth`;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';

  private readonly userSubject = new BehaviorSubject<{ username: string; rol: string } | null>(this.getUser());
  readonly user$ = this.userSubject.asObservable();

  login(request: RequestLogin): Observable<ResponseLogin> {
    return this.http.post<ResponseLogin>(`${this.baseUrl}/login`, request).pipe(
      tap((response) => {
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.USER_KEY, JSON.stringify({ username: response.username, rol: response.rol }));
        this.userSubject.next({ username: response.username, rol: response.rol });
      })
    );
  }

  logout(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/logout`, {}).pipe(
      finalize(() => {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        this.userSubject.next(null);
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUser(): { username: string; rol: string } | null {
    const user = localStorage.getItem(this.USER_KEY);
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
