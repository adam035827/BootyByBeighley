import { Routes } from '@angular/router';

export const coachDashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./shell/coach-shell.component').then(m => m.CoachShellComponent),
    children: [
      {
        path: 'students',
        loadComponent: () =>
          import('./pages/students/students.component').then(m => m.StudentsComponent),
      },
      {
        path: 'content',
        loadComponent: () =>
          import('./pages/content/content.component').then(m => m.ContentComponent),
      },
      {
        path: 'activity',
        loadComponent: () =>
          import('./pages/activity-feed/activity-feed.component').then(m => m.ActivityFeedComponent),
      },
      { path: '', redirectTo: 'students', pathMatch: 'full' },
    ],
  },
];
