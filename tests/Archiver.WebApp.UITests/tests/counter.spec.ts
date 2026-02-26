import { test, expect } from '@playwright/test';

test.describe('Counter', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the counter page
    await page.goto('/counter');
  });

  test('should display initial count as 0', async ({ page }) => {
    // Verify page title
    await expect(page).toHaveTitle('Counter');
    // Verify initial count
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 0');
  });

  test('should increment count when button is clicked', async ({ page }) => {
    // Verify initial state
    const statusText = page.locator('p[role="status"]');
    await expect(statusText).toContainText('Current count: 0');

    // Click button and verify increment
    await page.getByRole('button', { name: 'Click me' }).click();
    await expect(statusText).toContainText('Current count: 1');

    // Click again
    await page.getByRole('button', { name: 'Click me' }).click();
    await expect(statusText).toContainText('Current count: 2');
  });

  test('should reset count when navigating away and back', async ({ page }) => {
    // Increment count first
    await page.getByRole('button', { name: 'Click me' }).click();
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 1');

    // Navigate away to Home
    await page.goto('/');
    await expect(page).toHaveTitle('Home');

    // Navigate back to Counter
    await page.goto('/counter');
    await expect(page).toHaveTitle('Counter');

    // Verify count is reset
    await expect(page.locator('p[role="status"]')).toContainText('Current count: 0');
  });
});
