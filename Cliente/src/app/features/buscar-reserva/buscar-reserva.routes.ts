import { Routes } from '@angular/router';

export const BUSCAR_RESERVA_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./buscar-reserva').then(m => m.BuscarReserva) },
];
