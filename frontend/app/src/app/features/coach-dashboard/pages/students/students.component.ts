import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface StudentRosterItem {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  activePlanCount: number;
  lastWorkoutDate: string | null;
  hasPendingFeedback: boolean;
  hasUnreadPrs: boolean;
}

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss',
})
export class StudentsComponent implements OnInit {
  students: StudentRosterItem[] = [];
  loading = true;
  error: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadStudents();
  }

  private loadStudents() {
    this.http.get<StudentRosterItem[]>('/api/v1/coach/students').subscribe({
      next: (data) => {
        this.students = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load students: ' + (err?.message || 'Unknown error');
        this.loading = false;
      },
    });
  }

  getLastWorkoutDisplay(date: string | null): string {
    if (!date) return 'Never';
    const d = new Date(date);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (d.toDateString() === today.toDateString()) return 'Today';
    if (d.toDateString() === yesterday.toDateString()) return 'Yesterday';

    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
