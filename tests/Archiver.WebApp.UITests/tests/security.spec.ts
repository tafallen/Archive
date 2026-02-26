import { test, expect } from '@playwright/test';

test('should have security headers', async ({ page }) => {
  const response = await page.goto('/');
  expect(response).not.toBeNull();

  if (response === null) return;

  const headers = response.headers();

  // Check for X-Content-Type-Options
  expect(headers['x-content-type-options']).toBe('nosniff');

  // Check for X-Frame-Options
  expect(headers['x-frame-options']).toBe('SAMEORIGIN');

  // Check for Referrer-Policy
  expect(headers['referrer-policy']).toBe('strict-origin-when-cross-origin');

  // Check for Content-Security-Policy
  // Just checking existence for now, as the value might be complex
  expect(headers['content-security-policy']).toBeDefined();

  // Check for Permissions-Policy
  expect(headers['permissions-policy']).toBeDefined();
});
