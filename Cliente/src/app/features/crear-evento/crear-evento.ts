import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CrearEventoService } from './services';
import { RequestCrearEvento, Venue, TipoEvento } from './models';

@Component({
  selector: 'app-crear-evento',
  imports: [FormsModule],
  templateUrl: './crear-evento.html',
  styleUrl: './crear-evento.scss'
})
export class CrearEvento implements OnInit {
  private readonly crearEventoService = inject(CrearEventoService);
  readonly router = inject(Router);

  form: RequestCrearEvento = {
    titulo: '',
    descripcion: '',
    capacidadMaxima: 0,
    inicioEvento: '',
    iniciaHora: '',
    finEvento: '',
    finHora: '',
    precio: 0,
    venueId: 0,
    tipoEventoId: 0
  };

  venues = signal<Venue[]>([]);
  tiposEvento = signal<TipoEvento[]>([]);
  loading = signal(false);
  error = signal('');

  ngOnInit(): void {
    this.crearEventoService.getAllVenue().subscribe({
      next: (data) => this.venues.set(data),
      error: () => this.error.set('Error al cargar venues')
    });
    this.crearEventoService.getAllTipoEvento().subscribe({
      next: (data) => this.tiposEvento.set(data),
      error: () => this.error.set('Error al cargar tipos de evento')
    });
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.crearEventoService.create(this.form).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/buscar-evento']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Error al crear el evento');
      }
    });
  }
}
