import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PagarReservaService } from './services';
import { EstadoReservaEnum, RequestEstadoReserva } from './models';

@Component({
  selector: 'app-pagar-reserva',
  imports: [FormsModule],
  templateUrl: './pagar-reserva.html',
  styleUrl: './pagar-reserva.scss'
})
export class PagarReserva {
  private readonly pagarReservaService = inject(PagarReservaService);
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  form: RequestEstadoReserva = {
    estadoReservaEnum: EstadoReservaEnum.Confirmada,
    reservaId: 0
  };

  estados = [
    { value: EstadoReservaEnum.Confirmada, label: 'Confirmar Pago' },
    { value: EstadoReservaEnum.Cancelada, label: 'Cancelar Reserva' }
  ];

  loading = signal(false);
  error = signal('');
  successMessage = signal('');

  constructor() {
    const reservaId = this.route.snapshot.queryParamMap.get('reservaId');
    if (reservaId) {
      this.form.reservaId = +reservaId;
    }
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.successMessage.set('');
    this.pagarReservaService.payment(this.form).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.successMessage.set(result);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Error al procesar el pago');
      }
    });
  }
}
