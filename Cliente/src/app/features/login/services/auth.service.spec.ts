import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}/api/v1/Auth`;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should POST credentials and store token and user in localStorage', () => {
      const response = { token: 'jwt-token', username: 'admin', rol: 'Admin' };

      service.login({ username: 'admin', password: '1234' }).subscribe((res) => {
        expect(res).toEqual(response);
        expect(localStorage.getItem('auth_token')).toBe('jwt-token');
        expect(localStorage.getItem('auth_user')).toBe(JSON.stringify({ username: 'admin', rol: 'Admin' }));
      });

      const req = httpMock.expectOne(`${baseUrl}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'admin', password: '1234' });
      req.flush(response);
    });

    it('should emit user through user$ after login', () => {
      const response = { token: 'jwt-token', username: 'admin', rol: 'Admin' };
      let emittedUser: { username: string; rol: string } | null | undefined;

      service.user$.subscribe((user) => (emittedUser = user));
      expect(emittedUser).toBeNull();

      service.login({ username: 'admin', password: '1234' }).subscribe();
      httpMock.expectOne(`${baseUrl}/login`).flush(response);

      expect(emittedUser).toEqual({ username: 'admin', rol: 'Admin' });
    });
  });

  describe('logout', () => {
    it('should POST to logout endpoint', () => {
      localStorage.setItem('auth_token', 'jwt-token');
      localStorage.setItem('auth_user', JSON.stringify({ username: 'admin', rol: 'Admin' }));

      service.logout().subscribe();

      const req = httpMock.expectOne(`${baseUrl}/logout`);
      expect(req.request.method).toBe('POST');
      req.flush({ message: 'Sesión cerrada correctamente.' });
    });

    it('should clear localStorage and emit null user after logout', () => {
      localStorage.setItem('auth_token', 'jwt-token');
      localStorage.setItem('auth_user', JSON.stringify({ username: 'admin', rol: 'Admin' }));

      let emittedUser: { username: string; rol: string } | null | undefined;
      service.user$.subscribe((user) => (emittedUser = user));

      service.logout().subscribe();
      httpMock.expectOne(`${baseUrl}/logout`).flush({ message: 'OK' });

      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(localStorage.getItem('auth_user')).toBeNull();
      expect(emittedUser).toBeNull();
    });

    it('should clear localStorage even on logout error', () => {
      localStorage.setItem('auth_token', 'jwt-token');
      localStorage.setItem('auth_user', JSON.stringify({ username: 'admin', rol: 'Admin' }));

      service.logout().subscribe({ error: () => {} });
      httpMock.expectOne(`${baseUrl}/logout`).error(new ProgressEvent('error'), { status: 500 });

      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(localStorage.getItem('auth_user')).toBeNull();
    });
  });

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem('auth_token', 'test-token');
      expect(service.getToken()).toBe('test-token');
    });

    it('should return null when no token', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  describe('getUser', () => {
    it('should return parsed user from localStorage', () => {
      localStorage.setItem('auth_user', JSON.stringify({ username: 'admin', rol: 'Admin' }));
      expect(service.getUser()).toEqual({ username: 'admin', rol: 'Admin' });
    });

    it('should return null when no user', () => {
      expect(service.getUser()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem('auth_token', 'token');
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when no token', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
