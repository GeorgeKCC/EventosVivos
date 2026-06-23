import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { Navbar } from './navbar';
import { AuthService } from '../../features/login/services/auth.service';

describe('Navbar', () => {
  const userSubject = new BehaviorSubject<{ username: string; rol: string } | null>(null);

  const mockAuthService = {
    user$: userSubject.asObservable(),
    logout: vi.fn()
  };

  beforeEach(async () => {
    userSubject.next(null);

    await TestBed.configureTestingModule({
      imports: [Navbar],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    vi.clearAllMocks();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(Navbar);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show Login link when user is not authenticated', () => {
    const fixture = TestBed.createComponent(Navbar);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.user-info')).toBeNull();
    expect(el.textContent).toContain('Login');
  });

  it('should show username and logout button when user is authenticated', () => {
    userSubject.next({ username: 'admin', rol: 'Admin' });

    const fixture = TestBed.createComponent(Navbar);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.username')?.textContent).toContain('admin');
    expect(el.querySelector('.logout-btn')).toBeTruthy();
    expect(el.textContent).not.toContain('Login');
  });

  it('should call authService.logout and navigate to /login on logout', () => {
    userSubject.next({ username: 'admin', rol: 'Admin' });
    mockAuthService.logout.mockReturnValue(of({ message: 'OK' }));

    const fixture = TestBed.createComponent(Navbar);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const navigateSpy = vi.spyOn((component as any).router, 'navigate');

    component.onLogout();

    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('should hide username after logout emits null', () => {
    userSubject.next({ username: 'admin', rol: 'Admin' });

    const fixture = TestBed.createComponent(Navbar);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.username')).toBeTruthy();

    userSubject.next(null);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.username')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Login');
  });
});
