import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, defer, EMPTY, finalize, Subject, switchMap } from 'rxjs';
import { ActivityItemDto, Coach_DashboardClient } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';

type ActivityRange = 7 | 30;

@Component({
  selector: 'app-activity-feed',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './activity-feed.component.html',
  styleUrl: './activity-feed.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityFeedComponent implements OnInit {
  private readonly activityClient = inject(Coach_DashboardClient);
  private readonly errorService = inject(ErrorService);
  private readonly destroyRef = inject(DestroyRef);

  readonly activities = signal<ActivityItemDto[]>([]);
  readonly selectedRange = signal<ActivityRange>(7);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  private readonly loadRequests = new Subject<ActivityRange>();

  ngOnInit(): void {
    this.loadRequests
      .pipe(
        switchMap(range =>
          defer(() => {
            this.isLoading.set(true);
            this.errorMessage.set(null);

            return this.activityClient.getRecentActivity(range).pipe(
              catchError((error: HttpErrorResponse) => {
                this.errorMessage.set(this.errorService.extractMessage(error));
                return EMPTY;
              }),
              finalize(() => this.isLoading.set(false)),
            );
          }),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(activities => this.activities.set(activities));

    this.requestActivity();
  }

  selectRange(range: ActivityRange): void {
    if (range === this.selectedRange()) return;

    this.selectedRange.set(range);
    this.requestActivity();
  }

  refresh(): void {
    this.requestActivity();
  }

  private requestActivity(): void {
    this.loadRequests.next(this.selectedRange());
  }
}
