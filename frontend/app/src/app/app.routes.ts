import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'todo-items',
    loadChildren: () =>
      import('./features/todo-items/todo-items.routes').then(m => m.todoItemsRoutes),
  },
  { path: '', redirectTo: 'todo-items', pathMatch: 'full' },
];
