import { test, expect } from '@playwright/test';

test.describe('Counter', () => {
  test('should increment count when button is clicked', async ({ page }) => {
    // Navigate to the counter page
    await page.goto('/counter');

    // Verify page title
    await expect(page).toHaveTitle('Counter');

    // Verify initial count is 0
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 0');

    // Click the button and verify increment multiple times
    for (let i = 1; i <= 3; i++) {
        await page.getByRole('button', { name: 'Click me' }).click();
        await expect(page.locator('p[role="status"]')).toContainText(`Current count: ${i}`);
    }
  });
});
