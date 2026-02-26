import { test, expect } from '@playwright/test';

test.describe('Counter', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the counter page
    await page.goto('/counter');
  });

  test('should display initial count as 0', async ({ page }) => {
    // Verify page title
    await expect(page).toHaveTitle('Counter');

    // Verify initial count element presence and content
    const counterText = page.locator('p[role="status"]');
    await expect(counterText).toBeVisible();
    await expect(counterText).toHaveText('Current count: 0');
  });

  test('should increment count when button is clicked', async ({ page }) => {
    const counterText = page.locator('p[role="status"]');
    const button = page.getByRole('button', { name: 'Click me' });

    // Verify button is visible and enabled
    await expect(button).toBeVisible();
    await expect(button).toBeEnabled();

    // Click button and verify increment
    await button.click();
    await expect(counterText).toHaveText('Current count: 1');

    // Click again
    await button.click();
    await expect(counterText).toHaveText('Current count: 2');
  });

  test('should reset count when navigating away and back', async ({ page }) => {
    const button = page.getByRole('button', { name: 'Click me' });
    const counterText = page.locator('p[role="status"]');

    // Increment count first
    await button.click();
    await expect(counterText).toHaveText('Current count: 1');

    // Navigate away to Home via sidebar/nav
    // Note: Adjust selector based on actual navigation structure if needed,
    // but typically "Home" link exists.
    await page.getByRole('link', { name: 'Home' }).first().click();
    await expect(page).toHaveTitle('Home');

    // Navigate back to Counter
    await page.getByRole('link', { name: 'Counter' }).first().click();
    await expect(page).toHaveTitle('Counter');

    // Verify count is reset
    await expect(counterText).toHaveText('Current count: 0');
  });
});
