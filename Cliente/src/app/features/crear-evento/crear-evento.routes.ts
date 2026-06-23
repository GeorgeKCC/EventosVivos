import { Routes } from '@angular/router';

export const CREAR_EVENTO_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./crear-evento').then(m => m.CrearEvento) },
];
