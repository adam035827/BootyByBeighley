import { Routes } from '@angular/router';

export const todoItemsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/todo-list/todo-list.component').then(m => m.TodoListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./components/todo-form/todo-form.component').then(m => m.TodoFormComponent),
  },
  {
    // The :id route param is bound to the TodoFormComponent `id` input
    // via withComponentInputBinding() configured in app.config.ts.
    path: ':id/edit',
    loadComponent: () =>
      import('./components/todo-form/todo-form.component').then(m => m.TodoFormComponent),
  },
];
