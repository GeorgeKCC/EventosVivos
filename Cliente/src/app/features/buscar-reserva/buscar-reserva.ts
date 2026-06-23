import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BuscarReservaService } from './services';
import { ResponseReserva } from './models';

@Component({
  selector: 'app-buscar-reserva',
  imports: [RouterLink],
  templateUrl: './buscar-reserva.html',
  styleUrl: './buscar-reserva.scss'
})
export class BuscarReserva implements OnInit {
  private readonly buscarReservaService = inject(BuscarReservaService);

  reservas = signal<ResponseReserva[]>([]);
  loading = signal(false);

  ngOnInit(): void {
    this.loadReservas();
  }

  loadReservas(): void {
    this.loading.set(true);
    this.buscarReservaService.getAll().subscribe({
      next: (data) => {
        this.reservas.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
