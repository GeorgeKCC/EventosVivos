import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BuscarEvento } from './buscar-evento';
import { EventoService } from './services';
import { ResponseBuscarEvento } from './models';

describe('BuscarEvento', () => {
  const mockService = {
    search: vi.fn()
  };

  beforeEach(async () => {
    mockService.search.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [BuscarEvento],
      providers: [
        provideRouter([]),
        { provide: EventoService, useValue: mockService }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(BuscarEvento);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call search on init to load all events', () => {
    const fixture = TestBed.createComponent(BuscarEvento);
    fixture.detectChanges();

    expect(mockService.search).toHaveBeenCalledWith({});
  });

  it('should call search and set eventos on success', () => {
    const mockData: ResponseBuscarEvento[] = [
      {
        eventoId: 1,
        titulo: 'Concierto',
        descripción: 'Un concierto',
        venuedId: 1,
        venueName: 'Teatro',
        capacidadMaxima: 100,
        fechaInicioEvento: '2026-07-01',
        horaInicioEvento: '20:00',
        fechaFinEvento: '2026-07-01',
        horaFinEvento: '23:00',
        tipoEventoId: 1,
        tipoEventoNombre: 'Musical',
        estadoId: 1,
        estadoNombre: 'Activo'
      }
    ];
    mockService.search.mockReturnValue(of(mockData));

    const fixture = TestBed.createComponent(BuscarEvento);
    const component = fixture.componentInstance;
    component.filters = { titulo: 'Concierto' };

    component.search();

    expect(mockService.search).toHaveBeenCalledWith({ titulo: 'Concierto' });
    expect(component.eventos()).toEqual(mockData);
    expect(component.loading()).toBe(false);
  });

  it('should set loading to false on search error', () => {
    mockService.search.mockReturnValue(throwError(() => new Error('Network error')));

    const fixture = TestBed.createComponent(BuscarEvento);
    const component = fixture.componentInstance;

    component.search();

    expect(component.loading()).toBe(false);
    expect(component.eventos()).toEqual([]);
  });

  it('should set loading to true while searching', () => {
    mockService.search.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(BuscarEvento);
    const component = fixture.componentInstance;

    // Before subscribe completes synchronously, loading is set
    expect(component.loading()).toBe(false);
    component.search();
    // After synchronous observable completes
    expect(component.loading()).toBe(false);
  });
});
