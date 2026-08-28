import { Component, OnInit, signal, ChangeDetectorRef } from '@angular/core';
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
  students = signal<StudentRosterItem[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {
    console.log('StudentsComponent constructor executed');
  }

  ngOnInit() {
    console.log('StudentsComponent ngOnInit() called');
    this.loadStudents();
  }

  private loadStudents() {
    console.log('loadStudents() method called');
    const url = '/api/v1/coach/students';
    console.log('Making HTTP GET request to:', url);
    
    this.http.get<StudentRosterItem[]>(url).subscribe({
      next: (data) => {
        console.log('✓ HTTP response received');
        console.log('Response data:', data);
        console.log('Is array?', Array.isArray(data));
        console.log('Array length:', Array.isArray(data) ? data.length : 'N/A');
        
        // Update signals
        this.students.set(data);
        console.log('✓ Signal students.set() called with', data.length, 'items');
        
        this.loading.set(false);
        console.log('✓ Signal loading.set(false) called');
        
        // Explicit change detection as backup
        this.cdr.detectChanges();
        console.log('✓ detectChanges() called');
      },
      error: (err) => {
        console.error('✗ HTTP error occurred:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
        
        this.error.set('Failed to load students: ' + (err?.message || 'Unknown error'));
        this.loading.set(false);
        this.cdr.detectChanges();
        console.log('✗ Error handler completed, change detection triggered');
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
