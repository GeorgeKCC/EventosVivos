import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ReservaService } from './services';
import { RequestCrearReserva } from './models';

@Component({
  selector: 'app-crear-reserva',
  imports: [FormsModule],
  templateUrl: './crear-reserva.html',
  styleUrl: './crear-reserva.scss'
})
export class CrearReserva {
  private readonly reservaService = inject(ReservaService);
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  form: RequestCrearReserva = {
    eventoId: 0,
    cantidad: 1,
    nombreComprador: '',
    emailComprado: ''
  };

  loading = signal(false);
  error = signal('');

  constructor() {
    const eventoId = this.route.snapshot.queryParamMap.get('eventoId');
    if (eventoId) {
      this.form.eventoId = +eventoId;
    }
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.reservaService.create(this.form).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/buscar-evento']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Error al crear la reserva');
      }
    });
  }
}
