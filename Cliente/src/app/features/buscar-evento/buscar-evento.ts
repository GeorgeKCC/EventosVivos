import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { EventoService } from './services';
import { RequestBuscarEvento, ResponseBuscarEvento } from './models';

@Component({
  selector: 'app-buscar-evento',
  imports: [FormsModule, RouterLink],
  templateUrl: './buscar-evento.html',
  styleUrl: './buscar-evento.scss'
})
export class BuscarEvento {
  private readonly eventoService = inject(EventoService);

  filters: RequestBuscarEvento = {};
  eventos = signal<ResponseBuscarEvento[]>([]);
  loading = signal(false);

  search(): void {
    this.loading.set(true);
    this.eventoService.search(this.filters).subscribe({
      next: (data) => {
        this.eventos.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
