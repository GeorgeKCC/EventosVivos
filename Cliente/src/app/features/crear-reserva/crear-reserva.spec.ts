import { TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { Component } from '@angular/core';
import { of, throwError } from 'rxjs';
import { CrearReserva } from './crear-reserva';
import { ReservaService } from './services';

@Component({ template: '' })
class DummyComponent {}

describe('CrearReserva', () => {
  const mockService = {
    create: vi.fn()
  };

  const mockActivatedRoute = {
    snapshot: {
      queryParamMap: {
        get: vi.fn().mockReturnValue(null)
      }
    }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearReserva],
      providers: [
        provideRouter([{ path: 'buscar-evento', component: DummyComponent }]),
        { provide: ReservaService, useValue: mockService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    vi.clearAllMocks();
    mockActivatedRoute.snapshot.queryParamMap.get.mockReturnValue(null);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(CrearReserva);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize with default form values', () => {
    const fixture = TestBed.createComponent(CrearReserva);
    const component = fixture.componentInstance;

    expect(component.form.eventoId).toBe(0);
    expect(component.form.cantidad).toBe(1);
    expect(component.form.nombreComprador).toBe('');
    expect(component.form.emailComprado).toBe('');
  });

  it('should read eventoId from query params', () => {
    mockActivatedRoute.snapshot.queryParamMap.get.mockReturnValue('10');

    const fixture = TestBed.createComponent(CrearReserva);
    expect(fixture.componentInstance.form.eventoId).toBe(10);
  });

  it('should call create on submit and navigate on success', () => {
    mockService.create.mockReturnValue(of(undefined));

    const fixture = TestBed.createComponent(CrearReserva);
    const component = fixture.componentInstance;
    const navigateSpy = vi.spyOn(component.router, 'navigate');

    component.form.eventoId = 1;
    component.form.cantidad = 2;
    component.form.nombreComprador = 'Juan';
    component.form.emailComprado = 'juan@test.com';

    component.submit();

    expect(mockService.create).toHaveBeenCalledWith(component.form);
    expect(component.loading()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/buscar-evento']);
  });

  it('should set error on submit failure', () => {
    mockService.create.mockReturnValue(throwError(() => ({ error: { message: 'Sin disponibilidad' } })));

    const fixture = TestBed.createComponent(CrearReserva);
    const component = fixture.componentInstance;
    component.form.eventoId = 1;

    component.submit();

    expect(component.loading()).toBe(false);
    expect(component.error()).toBe('Sin disponibilidad');
  });

  it('should use default error message when no message provided', () => {
    mockService.create.mockReturnValue(throwError(() => ({})));

    const fixture = TestBed.createComponent(CrearReserva);
    const component = fixture.componentInstance;

    component.submit();

    expect(component.error()).toBe('Error al crear la reserva');
  });

  it('should clear error on new submit', () => {
    mockService.create.mockReturnValue(of(undefined));

    const fixture = TestBed.createComponent(CrearReserva);
    const component = fixture.componentInstance;
    component.error.set('previous error');

    component.submit();

    expect(component.error()).toBe('');
  });
});
