import { Routes } from '@angular/router';

export const CREAR_RESERVA_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./crear-reserva').then(m => m.CrearReserva) },
];
