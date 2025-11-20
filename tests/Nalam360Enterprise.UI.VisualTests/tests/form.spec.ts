import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('N360Form Component', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/components/form');
  });

  test('should render form with all fields', async ({ page }) => {
    const form = page.locator('form.n360-form').first();
    await expect(form).toBeVisible();
    
    const textInput = page.locator('input[type="text"]').first();
    await expect(textInput).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    const submitButton = page.locator('button[type="submit"]').first();
    await submitButton.click();
    
    // Check for validation errors
    const errorMessage = page.locator('.validation-error').first();
    await expect(errorMessage).toBeVisible();
  });

  test('should submit valid form', async ({ page }) => {
    await page.locator('input[name="firstName"]').fill('John');
    await page.locator('input[name="lastName"]').fill('Doe');
    await page.locator('input[name="email"]').fill('john.doe@example.com');
    
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    
    // Verify success message
    const successMessage = page.locator('.form-success');
    await expect(successMessage).toBeVisible({ timeout: 5000 });
  });

  test('should validate email format', async ({ page }) => {
    const emailInput = page.locator('input[name="email"]');
    await emailInput.fill('invalid-email');
    await emailInput.blur();
    
    const errorMessage = page.locator('.validation-error');
    await expect(errorMessage).toContainText(/email/i);
  });

  test('should be accessible', async ({ page }) => {
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });
});

test.describe('Form Visual Regression', () => {
  test('empty form', async ({ page }) => {
    await page.goto('/components/form');
    await expect(page).toHaveScreenshot('form-empty.png');
  });

  test('form with validation errors', async ({ page }) => {
    await page.goto('/components/form');
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    await expect(page).toHaveScreenshot('form-errors.png');
  });

  test('filled form', async ({ page }) => {
    await page.goto('/components/form');
    await page.locator('input[name="firstName"]').fill('John');
    await page.locator('input[name="lastName"]').fill('Doe');
    await page.locator('input[name="email"]').fill('john.doe@example.com');
    await expect(page).toHaveScreenshot('form-filled.png');
  });
});
