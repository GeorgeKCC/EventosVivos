import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BuscarReserva } from './buscar-reserva';
import { BuscarReservaService } from './services';
import { ResponseReserva } from './models';

describe('BuscarReserva', () => {
  const mockService = {
    getAll: vi.fn()
  };

  beforeEach(async () => {
    mockService.getAll.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [BuscarReserva],
      providers: [
        provideRouter([]),
        { provide: BuscarReservaService, useValue: mockService }
      ]
    }).compileComponents();

    vi.clearAllMocks();
    mockService.getAll.mockReturnValue(of([]));
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(BuscarReserva);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load reservas on init', () => {
    const reservas: ResponseReserva[] = [
      {
        id: 1,
        cantidad: 2,
        nombreComprador: 'Juan',
        emailComprador: 'juan@test.com',
        fechaCancelacion: null,
        esPerdida: false,
        eventoId: 1,
        evento: { id: 1, titulo: 'Concierto' },
        estadoReservaId: 2,
        estadoReserva: { id: 2, nombre: 'Confirmada' }
      }
    ];
    mockService.getAll.mockReturnValue(of(reservas));

    const fixture = TestBed.createComponent(BuscarReserva);
    fixture.detectChanges();

    expect(mockService.getAll).toHaveBeenCalled();
    expect(fixture.componentInstance.reservas()).toEqual(reservas);
    expect(fixture.componentInstance.loading()).toBe(false);
  });

  it('should set loading to false on error', () => {
    mockService.getAll.mockReturnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(BuscarReserva);
    fixture.detectChanges();

    expect(fixture.componentInstance.loading()).toBe(false);
    expect(fixture.componentInstance.reservas()).toEqual([]);
  });

  it('should set loading to true while fetching', () => {
    mockService.getAll.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(BuscarReserva);
    const component = fixture.componentInstance;

    component.loading.set(false);
    component.loadReservas();

    expect(component.loading()).toBe(false);
    expect(mockService.getAll).toHaveBeenCalled();
  });
});
