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
  const csp = headers['content-security-policy'];
  expect(csp).toBeDefined();
  expect(csp).toContain("default-src 'self'");
  // Ensure we are restricting frame ancestors to prevent clickjacking
  expect(csp).toContain("frame-ancestors 'self'");
  // Ensure we use a nonce
  expect(csp).toContain("'nonce-");

  // Check for Permissions-Policy
  expect(headers['permissions-policy']).toBeDefined();

  // Check for X-Permitted-Cross-Domain-Policies
  expect(headers['x-permitted-cross-domain-policies']).toBe('none');

  // Check for Cache-Control (allow either order of directives as middleware might modify them or differ based on env)
  expect(headers['cache-control']).toContain('no-store');
  expect(headers['cache-control']).toContain('no-cache');
  expect(headers['cache-control']).toContain('max-age=0');

  // Check for Pragma
  expect(headers['pragma']).toBe('no-cache');

  // X-XSS-Protection is deprecated and removed
  expect(headers['x-xss-protection']).toBeUndefined();
});
