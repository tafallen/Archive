import { test, expect } from '@playwright/test';

test.describe('Weather', () => {
  test('should load weather data and display table', async ({ page }) => {
    // Navigate to the weather page, but don't wait for full load as we want to catch the streaming state
    await page.goto('/weather', { waitUntil: 'commit' });

    // Verify page title
    await expect(page).toHaveTitle('Weather');

    // Verify loading state
    await expect(page.locator('text=Loading...')).toBeVisible();

    // Wait for the table to appear
    await expect(page.locator('table')).toBeVisible();

    // Verify loading message is gone
    await expect(page.locator('text=Loading...')).not.toBeVisible();

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

    const summaries = [
      "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    // Iterate through rows to validate data
    for (let i = 0; i < 5; i++) {
      const row = rows.nth(i);
      const cells = row.locator('td');

      // Date Validation
      const dateText = await cells.nth(0).textContent();
      expect(dateText).toBeTruthy();
      const date = new Date(dateText!);
      expect(date.toString()).not.toBe('Invalid Date');

      // Temp C Validation
      const tempCText = await cells.nth(1).textContent();
      const tempC = parseInt(tempCText!, 10);
      expect(tempC).toBeGreaterThanOrEqual(-20);
      expect(tempC).toBeLessThan(55);

      // Temp F Validation
      const tempFText = await cells.nth(2).textContent();
      const tempF = parseInt(tempFText!, 10);

      // Verify conversion: 32 + (int)(TemperatureC / 0.5556)
      // Note: Math.trunc behaves like C# (int) cast for positive/negative numbers (truncates towards zero)
      const expectedF = 32 + Math.trunc(tempC / 0.5556);
      expect(tempF).toBe(expectedF);

      // Summary Validation
      const summaryText = await cells.nth(3).textContent();
      expect(summaries).toContain(summaryText);
    }
  });
});
