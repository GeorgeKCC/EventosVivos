import { TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PagarReserva } from './pagar-reserva';
import { PagarReservaService } from './services';
import { EstadoReservaEnum } from './models';

describe('PagarReserva', () => {
  const mockService = {
    payment: vi.fn()
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
      imports: [PagarReserva],
      providers: [
        provideRouter([]),
        { provide: PagarReservaService, useValue: mockService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    vi.clearAllMocks();
    mockActivatedRoute.snapshot.queryParamMap.get.mockReturnValue(null);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(PagarReserva);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize with default form values', () => {
    const fixture = TestBed.createComponent(PagarReserva);
    const component = fixture.componentInstance;

    expect(component.form.estadoReservaEnum).toBe(EstadoReservaEnum.Confirmada);
    expect(component.form.reservaId).toBe(0);
  });

  it('should read reservaId from query params', () => {
    mockActivatedRoute.snapshot.queryParamMap.get.mockReturnValue('5');

    const fixture = TestBed.createComponent(PagarReserva);
    expect(fixture.componentInstance.form.reservaId).toBe(5);
  });

  it('should call payment on submit and set success message', () => {
    mockService.payment.mockReturnValue(of('Pago confirmado'));

    const fixture = TestBed.createComponent(PagarReserva);
    const component = fixture.componentInstance;
    component.form.reservaId = 1;

    component.submit();

    expect(mockService.payment).toHaveBeenCalledWith(component.form);
    expect(component.loading()).toBe(false);
    expect(component.successMessage()).toBe('Pago confirmado');
  });

  it('should set error on submit failure', () => {
    mockService.payment.mockReturnValue(throwError(() => ({ error: { message: 'Error de pago' } })));

    const fixture = TestBed.createComponent(PagarReserva);
    const component = fixture.componentInstance;
    component.form.reservaId = 1;

    component.submit();

    expect(component.loading()).toBe(false);
    expect(component.error()).toBe('Error de pago');
  });

  it('should clear previous messages on new submit', () => {
    mockService.payment.mockReturnValue(of('OK'));

    const fixture = TestBed.createComponent(PagarReserva);
    const component = fixture.componentInstance;
    component.error.set('old error');
    component.successMessage.set('old success');

    component.submit();

    expect(component.error()).toBe('');
  });
});
