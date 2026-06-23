import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CrearEvento } from './crear-evento';
import { CrearEventoService } from './services';

describe('CrearEvento', () => {
  const mockService = {
    create: vi.fn(),
    getAllVenue: vi.fn().mockReturnValue(of([])),
    getAllTipoEvento: vi.fn().mockReturnValue(of([]))
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearEvento],
      providers: [
        provideRouter([]),
        { provide: CrearEventoService, useValue: mockService }
      ]
    }).compileComponents();

    vi.clearAllMocks();
    mockService.getAllVenue.mockReturnValue(of([]));
    mockService.getAllTipoEvento.mockReturnValue(of([]));
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(CrearEvento);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load venues on init', () => {
    const venues = [{ id: 1, nombre: 'Venue 1', capacidad: 100, ciudad: 'Madrid' }];
    mockService.getAllVenue.mockReturnValue(of(venues));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();

    expect(mockService.getAllVenue).toHaveBeenCalled();
    expect(fixture.componentInstance.venues()).toEqual(venues);
  });

  it('should load tipos evento on init', () => {
    const tipos = [{ id: 1, nombre: 'Concierto' }];
    mockService.getAllTipoEvento.mockReturnValue(of(tipos));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();

    expect(mockService.getAllTipoEvento).toHaveBeenCalled();
    expect(fixture.componentInstance.tiposEvento()).toEqual(tipos);
  });

  it('should set error when venues fail to load', () => {
    mockService.getAllVenue.mockReturnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();

    expect(fixture.componentInstance.error()).toBe('Error al cargar venues');
  });

  it('should set error when tipos evento fail to load', () => {
    mockService.getAllTipoEvento.mockReturnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();

    expect(fixture.componentInstance.error()).toBe('Error al cargar tipos de evento');
  });

  it('should call create on submit and navigate on success', () => {
    mockService.create.mockReturnValue(of(undefined));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(fixture.componentInstance.router, 'navigate');

    fixture.componentInstance.form.titulo = 'Test Event';
    fixture.componentInstance.submit();

    expect(mockService.create).toHaveBeenCalledWith(fixture.componentInstance.form);
    expect(fixture.componentInstance.loading()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/buscar-evento']);
  });

  it('should set error on submit failure', () => {
    mockService.create.mockReturnValue(throwError(() => ({ error: { message: 'Error test' } })));

    const fixture = TestBed.createComponent(CrearEvento);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.loading()).toBe(false);
    expect(fixture.componentInstance.error()).toBe('Error test');
  });
});
