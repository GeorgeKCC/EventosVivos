import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './services';
import { RequestLogin } from './models';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  form: RequestLogin = {
    username: '',
    password: ''
  };

  loading = signal(false);
  error = signal('');

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.authService.login(this.form).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/buscar-evento']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Credenciales inválidas');
      }
    });
  }
}
