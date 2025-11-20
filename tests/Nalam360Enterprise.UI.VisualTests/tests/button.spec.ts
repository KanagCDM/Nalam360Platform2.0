import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('N360Button Component', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/components/button');
  });

  test('should render primary button', async ({ page }) => {
    const button = page.locator('button.n360-button-primary').first();
    await expect(button).toBeVisible();
    await expect(button).toHaveText(/Primary/i);
  });

  test('should take screenshot of all button variants', async ({ page }) => {
    await expect(page.locator('.button-showcase')).toBeVisible();
    await page.screenshot({ 
      path: 'screenshots/button-variants.png',
      fullPage: true 
    });
  });

  test('should be accessible', async ({ page }) => {
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('should handle click interactions', async ({ page }) => {
    const button = page.locator('button.n360-button-primary').first();
    await button.click();
    
    // Verify click event
    const clickedIndicator = page.locator('.button-clicked');
    await expect(clickedIndicator).toBeVisible();
  });

  test('should support keyboard navigation', async ({ page }) => {
    await page.keyboard.press('Tab');
    const focusedButton = page.locator('button:focus');
    await expect(focusedButton).toBeVisible();
    
    await page.keyboard.press('Enter');
    const clickedIndicator = page.locator('.button-clicked');
    await expect(clickedIndicator).toBeVisible();
  });

  test('should render disabled state', async ({ page }) => {
    const disabledButton = page.locator('button[disabled]').first();
    await expect(disabledButton).toBeDisabled();
    await expect(disabledButton).toHaveClass(/disabled/);
  });
});

test.describe('Visual Regression - Button States', () => {
  test('default state', async ({ page }) => {
    await page.goto('/components/button');
    await expect(page).toHaveScreenshot('button-default.png');
  });

  test('hover state', async ({ page }) => {
    await page.goto('/components/button');
    const button = page.locator('button.n360-button-primary').first();
    await button.hover();
    await expect(page).toHaveScreenshot('button-hover.png');
  });

  test('focus state', async ({ page }) => {
    await page.goto('/components/button');
    await page.keyboard.press('Tab');
    await expect(page).toHaveScreenshot('button-focus.png');
  });

  test('loading state', async ({ page }) => {
    await page.goto('/components/button');
    const loadingButton = page.locator('button.loading').first();
    await expect(loadingButton).toBeVisible();
    await expect(page).toHaveScreenshot('button-loading.png');
  });
});
