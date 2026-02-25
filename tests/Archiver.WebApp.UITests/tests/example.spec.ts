import { test, expect } from '@playwright/test';

test('has title', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle('Home');
});

test('has a counter', async ({ page }) => {
  await page.goto('/counter');
  await expect(page.locator('h1')).toContainText('Counter');
});
