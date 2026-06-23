import { Routes } from '@angular/router';

export const PAGAR_RESERVA_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pagar-reserva').then(m => m.PagarReserva) },
];
