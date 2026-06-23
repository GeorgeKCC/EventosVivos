import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Reportes } from './reportes';
import { ReporteService } from './services';
import { TipoReporte } from './models';

describe('Reportes', () => {
  const mockService = {
    downloadReport: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Reportes],
      providers: [
        provideRouter([]),
        { provide: ReporteService, useValue: mockService }
      ]
    }).compileComponents();

    vi.clearAllMocks();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(Reportes);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize with default form values', () => {
    const fixture = TestBed.createComponent(Reportes);
    const component = fixture.componentInstance;

    expect(component.form.eventoId).toBe(0);
    expect(component.form.tipoReporte).toBe(TipoReporte.TotalEntradas);
  });

  it('should have all report types', () => {
    const fixture = TestBed.createComponent(Reportes);
    const component = fixture.componentInstance;

    expect(component.tiposReporte).toHaveLength(5);
    expect(component.tiposReporte.map(t => t.value)).toEqual([
      TipoReporte.TotalEntradas,
      TipoReporte.TotalEntradasDisponibles,
      TipoReporte.PorcentajeDeOcupacion,
      TipoReporte.TotalDeIngresos,
      TipoReporte.EstadoDelEvento
    ]);
  });

  it('should download report and trigger file download', () => {
    const blob = new Blob(['test'], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    mockService.downloadReport.mockReturnValue(of(blob));

    const fixture = TestBed.createComponent(Reportes);
    const component = fixture.componentInstance;
    component.form.eventoId = 1;

    const createObjectURLMock = vi.fn().mockReturnValue('blob:test');
    const revokeObjectURLMock = vi.fn();
    window.URL.createObjectURL = createObjectURLMock;
    window.URL.revokeObjectURL = revokeObjectURLMock;

    const clickMock = vi.fn();
    const originalCreateElement = document.createElement.bind(document);
    vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
      if (tag === 'a') {
        return { href: '', download: '', click: clickMock } as any;
      }
      return originalCreateElement(tag);
    });

    component.download();

    expect(mockService.downloadReport).toHaveBeenCalledWith(component.form);
    expect(component.loading()).toBe(false);
    expect(createObjectURLMock).toHaveBeenCalledWith(blob);
    expect(clickMock).toHaveBeenCalled();
    expect(revokeObjectURLMock).toHaveBeenCalledWith('blob:test');
  });

  it('should set error on download failure', () => {
    mockService.downloadReport.mockReturnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(Reportes);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.download();

    expect(component.loading()).toBe(false);
    expect(component.error()).toBe('Error al descargar el reporte');
  });
});
