import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../features/login/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class Navbar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = toSignal(this.authService.user$);

  onLogout(): void {
    this.authService.logout().subscribe({
      complete: () => this.router.navigate(['/login'])
    });
  }
}
