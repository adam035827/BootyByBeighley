import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StudentsComponent } from './students.component';

const STUDENTS_URL = '/api/v1/coach/students';

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

describe('StudentsComponent', () => {
  let fixture: ComponentFixture<StudentsComponent>;
  let httpMock: HttpTestingController;

  const student: StudentRosterItem = {
    id: 'student-1',
    email: 'jamie@example.com',
    firstName: 'Jamie',
    lastName: 'Student',
    activePlanCount: 2,
    lastWorkoutDate: null,
    hasPendingFeedback: false,
    hasUnreadPrs: false,
  };

  function daysAgo(days: number): Date {
    const date = new Date();
    date.setDate(date.getDate() - days);
    return date;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(StudentsComponent);
    fixture.detectChanges();
  }

  function respondWith(roster: StudentRosterItem[]): void {
    httpMock.expectOne(STUDENTS_URL).flush(roster);
    fixture.detectChanges();
  }

  it('requests the coach roster once on init and shows the loading state until it responds', () => {
    createComponent();

    const request = httpMock.expectOne(STUDENTS_URL);
    expect(request.request.method).toBe('GET');
    expect(fixture.componentInstance.loading()).toBe(true);
    expect(fixture.nativeElement.querySelector('.students__loading')?.textContent).toContain(
      'Loading students...',
    );

    request.flush([]);
    fixture.detectChanges();

    expect(fixture.componentInstance.loading()).toBe(false);
    expect(fixture.nativeElement.querySelector('.students__loading')).toBeNull();
  });

  it('renders the roster returned by the API', () => {
    createComponent();

    respondWith([
      student,
      {
        ...student,
        id: 'student-2',
        email: 'alex@example.com',
        firstName: 'Alex',
        lastName: 'Lifter',
        activePlanCount: 1,
        hasPendingFeedback: true,
        hasUnreadPrs: true,
      },
    ]);

    const cards = fixture.nativeElement.querySelectorAll('.student-card');
    expect(cards).toHaveLength(2);
    expect(cards[0].querySelector('.student-card__title')?.textContent).toContain('Jamie Student');
    expect(cards[0].querySelector('.student-card__email')?.textContent).toContain('jamie@example.com');
    expect(cards[0].querySelectorAll('.stat__value')[0]?.textContent).toContain('2');
    expect(cards[0].querySelectorAll('.badge')).toHaveLength(0);

    expect(cards[1].querySelector('.badge--feedback')?.textContent).toContain('Feedback');
    expect(cards[1].querySelector('.badge--pr')?.textContent).toContain('New PR');
    expect(fixture.nativeElement.querySelector('.students__empty')).toBeNull();
    expect(fixture.nativeElement.querySelector('.students__error')).toBeNull();
  });

  it('renders the relative last workout date for each student', () => {
    createComponent();

    respondWith([{ ...student, lastWorkoutDate: daysAgo(1).toISOString() }]);

    expect(fixture.nativeElement.querySelectorAll('.stat__value')[1]?.textContent).toContain('Yesterday');
  });

  it('shows the empty state when the roster is empty', () => {
    createComponent();

    respondWith([]);

    expect(fixture.componentInstance.students()).toEqual([]);
    expect(fixture.nativeElement.querySelector('.students__empty')?.textContent).toContain(
      'No students enrolled yet.',
    );
    expect(fixture.nativeElement.querySelectorAll('.student-card')).toHaveLength(0);
  });

  it('shows the error state when the request fails', () => {
    createComponent();

    httpMock
      .expectOne(STUDENTS_URL)
      .flush('boom', { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();

    expect(fixture.componentInstance.loading()).toBe(false);
    expect(fixture.componentInstance.error()).toContain('Failed to load students:');
    expect(fixture.nativeElement.querySelector('.students__error')?.textContent).toContain(
      'Failed to load students:',
    );
    expect(fixture.nativeElement.querySelector('.students__empty')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('.student-card')).toHaveLength(0);
  });

  describe('getLastWorkoutDisplay', () => {
    let component: StudentsComponent;

    beforeEach(() => {
      createComponent();
      respondWith([]);
      component = fixture.componentInstance;
    });

    it('returns "Never" when there is no date', () => {
      expect(component.getLastWorkoutDisplay(null)).toBe('Never');
      expect(component.getLastWorkoutDisplay('')).toBe('Never');
    });

    it('returns "Today" for a date earlier today', () => {
      expect(component.getLastWorkoutDisplay(new Date().toISOString())).toBe('Today');
    });

    it('returns "Yesterday" for a date one day ago', () => {
      expect(component.getLastWorkoutDisplay(daysAgo(1).toISOString())).toBe('Yesterday');
    });

    it('falls back to a formatted date for older workouts', () => {
      const older = daysAgo(30);

      expect(component.getLastWorkoutDisplay(older.toISOString())).toBe(
        older.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
      );
    });
  });
});
