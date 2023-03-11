import { test, expect } from '@playwright/test';

test('basic test', async ({ page }) => {
  await page.goto('/');
  await page.waitForLoadState('networkidle');
  await page.waitForSelector('h1', {timeout: 60000});
  await expect(page.locator('h1')).toContainText('Hello, world!');
})