import { test, expect } from '@playwright/test';

test.describe('Counter', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the counter page
    await page.goto('/counter');
  });

  test('should display initial count as 0', async ({ page }) => {
    await expect(page).toHaveTitle('Counter');
    const counterText = page.locator('p[role="status"]');
    await expect(counterText).toBeVisible();
    await expect(counterText).toHaveText('Current count: 0');
  });

  test('should increment count when button is clicked', async ({ page }) => {
    const counterText = page.locator('p[role="status"]');
    const button = page.getByRole('button', { name: 'Click me' });

    await expect(button).toBeVisible();
    await expect(button).toBeEnabled();

    // Verify increment with retry to handle Blazor Server hydration
    await expect(async () => {
      const text = await counterText.innerText();
      // Only click if we haven't incremented yet
      if (text.includes('Current count: 0')) {
          await button.click();
      }
      // Short timeout for the check to allow retries
      await expect(counterText).toHaveText('Current count: 1', { timeout: 1000 });
    }).toPass({ timeout: 10000 });

    // Click again - should work immediately now
    await button.click();
    await expect(counterText).toHaveText('Current count: 2');
  });

  test('should reset count when navigating away and back', async ({ page }) => {
    const button = page.getByRole('button', { name: 'Click me' });
    const counterText = page.locator('p[role="status"]');

    // Ensure initial increment works (using the same robust pattern)
    await expect(async () => {
      const text = await counterText.innerText();
      if (text.includes('Current count: 0')) {
          await button.click();
      }
      await expect(counterText).toHaveText('Current count: 1', { timeout: 1000 });
    }).toPass({ timeout: 10000 });

    // Navigate away to Home
    await page.getByRole('link', { name: 'Home' }).first().click();
    await expect(page).toHaveTitle('Home');

    // Navigate back to Counter
    await page.getByRole('link', { name: 'Counter' }).first().click();
    await expect(page).toHaveTitle('Counter');

    // Verify count is reset
    await expect(counterText).toHaveText('Current count: 0');
  });
});
