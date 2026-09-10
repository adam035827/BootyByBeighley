import { expect, test } from '@playwright/test';

test('coach filters and refreshes recent activity', async ({ page }) => {
  const requestedRanges: string[] = [];

  await page.route('**/api/v1/coach/activity**', async route => {
    const range = new URL(route.request().url()).searchParams.get('limitDays') ?? '';
    requestedRanges.push(range);

    await route.fulfill({
      contentType: 'application/json',
      json: [
        {
          id: `activity-${range}`,
          userId: 'student-1',
          studentName: 'Jamie Student',
          studentEmail: 'jamie@example.com',
          workoutName: range === '30' ? 'Thirty Day Workout' : 'Seven Day Workout',
          completedAt: '2026-08-30T14:30:00Z',
          totalSets: 12,
          totalMovements: 4,
        },
      ],
    });
  });

  await page.goto('/coach/activity');

  await expect(page.getByRole('heading', { name: 'Seven Day Workout' })).toBeVisible();
  await page.getByRole('button', { name: '30 days' }).click();
  await expect(page.getByRole('heading', { name: 'Thirty Day Workout' })).toBeVisible();
  await expect(page.getByRole('button', { name: '30 days' })).toHaveAttribute('aria-pressed', 'true');

  await page.getByRole('button', { name: 'Refresh' }).click();
  await expect.poll(() => requestedRanges).toEqual(['7', '30', '30']);
});