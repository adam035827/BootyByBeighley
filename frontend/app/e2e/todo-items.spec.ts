import { test, expect } from '@playwright/test';

test.describe('Todo items CRUD', () => {
  const title = `E2E todo ${Date.now()}`;
  const description = 'E2E description';
  const updatedTitle = `${title} (updated)`;
  const updatedDescription = 'Updated description';

  test('creates, updates, completes, and deletes a todo item', async ({ page }) => {
    await page.goto('/todo-items');

    await page.click('text=Add Item');
    await expect(page.locator('h2')).toHaveText('New Item');

    await page.fill('#title', title);
    await page.fill('#description', description);
    await page.click('button:has-text("Create Item")');

    await expect(page).toHaveURL(/\/todo-items$/);
    const item = page.locator('.todo-list__item', { hasText: title });
    await expect(item).toBeVisible();
    await expect(page.locator('.todo-list__badge')).toContainText('pending');

    await item.locator('a:has-text("Edit")').click();
    await expect(page.locator('h2')).toHaveText('Edit Item');

    await page.fill('#title', updatedTitle);
    await page.fill('#description', updatedDescription);
    await page.click('button:has-text("Save Changes")');

    await expect(page).toHaveURL(/\/todo-items$/);
    const updatedItem = page.locator('.todo-list__item', { hasText: updatedTitle });
    await expect(updatedItem).toBeVisible();

    await updatedItem.locator('input[type="checkbox"]').check();
    await expect(updatedItem).toHaveClass(/todo-list__item--done/);

    await updatedItem.locator('button:has-text("Delete")').click();
    await expect(page.locator('.todo-list__item', { hasText: updatedTitle })).toHaveCount(0);
  });
});
