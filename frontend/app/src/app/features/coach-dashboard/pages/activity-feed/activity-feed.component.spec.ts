import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { ActivityItemDto, Coach_DashboardClient } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';
import { ActivityFeedComponent } from './activity-feed.component';

describe('ActivityFeedComponent', () => {
  let fixture: ComponentFixture<ActivityFeedComponent>;
  let activityClient: { getRecentActivity: ReturnType<typeof vi.fn> };
  let errorService: { extractMessage: ReturnType<typeof vi.fn> };

  const activity: ActivityItemDto = {
    id: 'activity-1',
    userId: 'student-1',
    studentName: 'Jamie Student',
    studentEmail: 'jamie@example.com',
    workoutName: 'Lower Body Strength',
    completedAt: '2026-08-30T14:30:00Z',
    totalSets: 12,
    totalMovements: 4,
  };

  beforeEach(async () => {
    activityClient = { getRecentActivity: vi.fn() };
    errorService = { extractMessage: vi.fn().mockReturnValue('Could not load activity.') };

    await TestBed.configureTestingModule({
      imports: [ActivityFeedComponent],
      providers: [
        { provide: Coach_DashboardClient, useValue: activityClient },
        { provide: ErrorService, useValue: errorService },
      ],
    }).compileComponents();
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(ActivityFeedComponent);
    fixture.detectChanges();
  }

  it('loads and renders the last seven days of activity', () => {
    activityClient.getRecentActivity.mockReturnValue(of([activity]));

    createComponent();

    expect(activityClient.getRecentActivity).toHaveBeenCalledWith(7);
    expect(fixture.nativeElement.querySelector('.activity-item__student')?.textContent).toContain('Jamie Student');
    expect(fixture.nativeElement.querySelector('.activity-item__workout')?.textContent).toContain('Lower Body Strength');
  });

  it('reloads activity when the range changes', () => {
    activityClient.getRecentActivity.mockReturnValue(of([activity]));
    createComponent();

    fixture.nativeElement.querySelectorAll('.activity-feed__filters button')[1].click();
    fixture.detectChanges();

    expect(activityClient.getRecentActivity).toHaveBeenLastCalledWith(30);
    expect(fixture.componentInstance.selectedRange()).toBe(30);
  });

  it('ignores a stale response after the range changes', () => {
    const sevenDayResponse = new Subject<ActivityItemDto[]>();
    const thirtyDayResponse = new Subject<ActivityItemDto[]>();
    const thirtyDayActivity = { ...activity, id: 'activity-30', workoutName: 'Thirty Day Workout' };
    activityClient.getRecentActivity
      .mockReturnValueOnce(sevenDayResponse)
      .mockReturnValueOnce(thirtyDayResponse);
    createComponent();

    fixture.nativeElement.querySelectorAll('.activity-feed__filters button')[1].click();
    thirtyDayResponse.next([thirtyDayActivity]);
    thirtyDayResponse.complete();
    fixture.detectChanges();
    sevenDayResponse.next([activity]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.activity-item__workout')?.textContent).toContain('Thirty Day Workout');
  });

  it('shows an empty state for the selected range', () => {
    activityClient.getRecentActivity.mockReturnValue(of([]));

    createComponent();

    expect(fixture.nativeElement.querySelector('.activity-feed__state')?.textContent).toContain(
      'No workout activity in the last 7 days.',
    );
  });

  it('shows a friendly error and supports retrying', () => {
    const responseError = new HttpErrorResponse({ status: 500 });
    activityClient.getRecentActivity
      .mockReturnValueOnce(throwError(() => responseError))
      .mockReturnValueOnce(of([activity]));
    createComponent();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Could not load activity.');

    fixture.nativeElement.querySelector('[role="alert"] button').click();
    fixture.detectChanges();

    expect(activityClient.getRecentActivity).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelectorAll('.activity-item')).toHaveLength(1);
  });
});