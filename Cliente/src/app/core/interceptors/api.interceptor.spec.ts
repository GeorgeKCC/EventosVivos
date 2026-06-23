import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';
import { apiInterceptor } from './api.interceptor';

describe('apiInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiInterceptor])),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should add X-Api-Key header to non-login requests', () => {
    http.get('/api/v1/Evento').subscribe();

    const req = httpMock.expectOne('/api/v1/Evento');
    expect(req.request.headers.get('X-Api-Key')).toBe('EV-2024-SecureKey-a1b2c3d4e5f6');
    req.flush([]);
  });

  it('should not add headers to login requests', () => {
    http.post('/api/v1/Auth/login', {}).subscribe();

    const req = httpMock.expectOne('/api/v1/Auth/login');
    expect(req.request.headers.has('X-Api-Key')).toBe(false);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should add Authorization Bearer header when token exists', () => {
    localStorage.setItem('auth_token', 'my-jwt-token');

    http.get('/api/v1/Evento').subscribe();

    const req = httpMock.expectOne('/api/v1/Evento');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt-token');
    expect(req.request.headers.get('X-Api-Key')).toBe('EV-2024-SecureKey-a1b2c3d4e5f6');
    req.flush([]);
  });

  it('should not add Authorization header when no token exists', () => {
    http.get('/api/v1/Evento').subscribe();

    const req = httpMock.expectOne('/api/v1/Evento');
    expect(req.request.headers.has('Authorization')).toBe(false);
    expect(req.request.headers.get('X-Api-Key')).toBe('EV-2024-SecureKey-a1b2c3d4e5f6');
    req.flush([]);
  });
});
