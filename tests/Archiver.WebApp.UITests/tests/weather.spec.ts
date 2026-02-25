import { test, expect } from '@playwright/test';

test.describe('Weather', () => {
  test('should load weather data and display table', async ({ page }) => {
    // Navigate to the weather page
    await page.goto('/weather');

    // Verify page title
    await expect(page).toHaveTitle('Weather');

    // Wait for the table to appear
    await expect(page.locator('table')).toBeVisible();

    // Verify table headers
    const headers = page.locator('table thead tr th');
    await expect(headers).toHaveCount(4);
    await expect(headers.nth(0)).toHaveText('Date');
    await expect(headers.nth(1)).toHaveText('Temp. (C)');
    await expect(headers.nth(2)).toHaveText('Temp. (F)');
    await expect(headers.nth(3)).toHaveText('Summary');

    // Verify table has 5 rows of data
    const rows = page.locator('table tbody tr');
    await expect(rows).toHaveCount(5);
  });
});
