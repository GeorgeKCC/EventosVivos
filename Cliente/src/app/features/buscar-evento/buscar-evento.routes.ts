import { Routes } from '@angular/router';

export const BUSCAR_EVENTO_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./buscar-evento').then(m => m.BuscarEvento) },
];
