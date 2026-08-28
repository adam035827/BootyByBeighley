import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'coach',
    loadChildren: () =>
      import('./features/coach-dashboard/coach-dashboard.routes').then(m => m.coachDashboardRoutes),
  },
  { path: '', redirectTo: 'coach', pathMatch: 'full' },
];
