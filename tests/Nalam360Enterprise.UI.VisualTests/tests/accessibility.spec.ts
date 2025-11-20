import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Accessibility Tests', () => {
  const pages = [
    '/dashboard',
    '/patients',
    '/appointments',
    '/clinical-decision-support',
    '/analytics',
    '/components/button',
    '/components/grid',
    '/components/form',
    '/components/chart',
  ];

  for (const pagePath of pages) {
    test(`${pagePath} should be accessible`, async ({ page }) => {
      await page.goto(pagePath);
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test(`${pagePath} should support keyboard navigation`, async ({ page }) => {
      await page.goto(pagePath);
      
      // Tab through interactive elements
      await page.keyboard.press('Tab');
      const focusedElement = page.locator(':focus');
      await expect(focusedElement).toBeVisible();
      
      // Verify focus indicator
      await expect(focusedElement).toHaveCSS('outline-width', /[1-9]/);
    });

    test(`${pagePath} should have proper heading hierarchy`, async ({ page }) => {
      await page.goto(pagePath);
      
      const h1Count = await page.locator('h1').count();
      expect(h1Count).toBe(1); // Only one H1 per page
      
      // Verify no skipped heading levels
      const headings = await page.locator('h1, h2, h3, h4, h5, h6').all();
      const levels = await Promise.all(
        headings.map(async (h) => {
          const tagName = await h.evaluate((el) => el.tagName);
          return parseInt(tagName.substring(1));
        })
      );
      
      for (let i = 1; i < levels.length; i++) {
        const diff = levels[i] - levels[i - 1];
        expect(diff).toBeLessThanOrEqual(1); // No skipped levels
      }
    });
  }

  test('should support screen reader announcements', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Check for ARIA live regions
    const liveRegions = page.locator('[aria-live]');
    const count = await liveRegions.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should have proper ARIA labels', async ({ page }) => {
    await page.goto('/components/button');
    
    const buttons = page.locator('button');
    const buttonCount = await buttons.count();
    
    for (let i = 0; i < buttonCount; i++) {
      const button = buttons.nth(i);
      const hasText = await button.textContent().then(text => text && text.trim().length > 0);
      const hasAriaLabel = await button.getAttribute('aria-label').then(label => label !== null);
      const hasAriaLabelledby = await button.getAttribute('aria-labelledby').then(id => id !== null);
      
      expect(hasText || hasAriaLabel || hasAriaLabelledby).toBeTruthy();
    }
  });

  test('should have sufficient color contrast', async ({ page }) => {
    await page.goto('/dashboard');
    
    const contrastResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .include('body')
      .analyze();

    const contrastViolations = contrastResults.violations.filter(
      v => v.id === 'color-contrast'
    );
    
    expect(contrastViolations).toEqual([]);
  });
});

test.describe('High Contrast Mode', () => {
  test.use({ 
    colorScheme: 'dark'
  });

  test('should work in high contrast mode', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Emulate high contrast mode via CSS media query
    await page.emulateMedia({ colorScheme: 'dark', forcedColors: 'active' });
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aaa'])
      .analyze();

    expect(accessibilityScanResults.violations.length).toBeLessThan(5);
  });
});

test.describe('RTL Support', () => {
  test('should support right-to-left layout', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Enable RTL mode
    await page.evaluate(() => {
      document.documentElement.setAttribute('dir', 'rtl');
    });
    
    await page.waitForTimeout(500);
    
    // Verify layout direction
    const bodyDir = await page.locator('body').getAttribute('dir');
    expect(bodyDir).toBe('rtl');
    
    // Take screenshot
    await expect(page).toHaveScreenshot('dashboard-rtl.png');
  });
});
