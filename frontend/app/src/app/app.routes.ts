import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'coach',
    loadChildren: () =>
      import('./features/coach-dashboard/coach-dashboard.routes').then(m => m.coachDashboardRoutes),
  },
  {
    path: 'todo-items',
    loadChildren: () =>
      import('./features/todo-items/todo-items.routes').then(m => m.todoItemsRoutes),
  },
  { path: '', redirectTo: 'coach', pathMatch: 'full' },
];
