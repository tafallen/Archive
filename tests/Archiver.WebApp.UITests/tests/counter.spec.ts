import { test, expect } from '@playwright/test';

test.describe('Counter', () => {
  test('should increment count when button is clicked', async ({ page }) => {
    // Navigate to the counter page
    await page.goto('/counter');

    // Verify initial count is 0
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 0');

    // Click the button
    await page.getByRole('button', { name: 'Click me' }).click();

    // Verify count increments to 1
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 1');

    // Click the button again
    await page.getByRole('button', { name: 'Click me' }).click();

    // Verify count increments to 2
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 2');
  });
});
