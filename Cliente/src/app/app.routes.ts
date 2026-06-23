import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'buscar-evento', pathMatch: 'full' },
  { path: 'buscar-evento', loadChildren: () => import('./features/buscar-evento/buscar-evento.routes').then(m => m.BUSCAR_EVENTO_ROUTES) },
  { path: 'crear-evento', loadChildren: () => import('./features/crear-evento/crear-evento.routes').then(m => m.CREAR_EVENTO_ROUTES) },
  { path: 'crear-reserva', loadChildren: () => import('./features/crear-reserva/crear-reserva.routes').then(m => m.CREAR_RESERVA_ROUTES) },
  { path: 'pagar-reserva', loadChildren: () => import('./features/pagar-reserva/pagar-reserva.routes').then(m => m.PAGAR_RESERVA_ROUTES) },
  { path: 'buscar-reserva', loadChildren: () => import('./features/buscar-reserva/buscar-reserva.routes').then(m => m.BUSCAR_RESERVA_ROUTES) },
  { path: 'reportes', loadChildren: () => import('./features/reportes/reportes.routes').then(m => m.REPORTES_ROUTES) },
];
