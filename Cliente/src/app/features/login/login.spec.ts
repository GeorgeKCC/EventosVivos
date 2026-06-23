import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { AuthService } from './services';

describe('Login', () => {
  const mockService = {
    login: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockService }
      ]
    }).compileComponents();

    vi.clearAllMocks();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(Login);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize with empty form', () => {
    const fixture = TestBed.createComponent(Login);
    const component = fixture.componentInstance;

    expect(component.form.username).toBe('');
    expect(component.form.password).toBe('');
  });

  it('should call login and navigate on success', () => {
    mockService.login.mockReturnValue(of({ token: 'abc123', username: 'admin', rol: 'Admin' }));

    const fixture = TestBed.createComponent(Login);
    const component = fixture.componentInstance;
    const navigateSpy = vi.spyOn((component as any).router, 'navigate');

    component.form.username = 'admin';
    component.form.password = 'password';
    component.submit();

    expect(mockService.login).toHaveBeenCalledWith({ username: 'admin', password: 'password' });
    expect(component.loading()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/buscar-evento']);
  });

  it('should set error on login failure', () => {
    mockService.login.mockReturnValue(throwError(() => ({ error: { message: 'Credenciales inválidas' } })));

    const fixture = TestBed.createComponent(Login);
    const component = fixture.componentInstance;

    component.form.username = 'admin';
    component.form.password = 'wrong';
    component.submit();

    expect(component.loading()).toBe(false);
    expect(component.error()).toBe('Credenciales inválidas');
  });
});
